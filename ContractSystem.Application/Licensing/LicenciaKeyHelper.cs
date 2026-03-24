using System.Security.Cryptography;
using System.Text;

namespace ContractSystem.Application.Licensing;

/// <summary>
/// Utilidades compartidas para generación y validación de claves de licencia.
/// El fingerprint se calcula como SHA256(service_broker_guid + "|" + create_date).
/// La clave de licencia es un token firmado con HMAC-SHA256 que contiene fingerprint + fecha expiración.
/// </summary>
public static class LicenciaKeyHelper
{
    // Clave secreta para HMAC — dispersa para dificultar extracción por decompilación.
    private static byte[] GetSecretKey()
    {
        var p1 = new byte[] { 0x59, 0x56, 0x54, 0x2D, 0x43, 0x6F, 0x6E, 0x74 }; // YVT-Cont
        var p2 = new byte[] { 0x72, 0x61, 0x63, 0x74, 0x53, 0x79, 0x73, 0x74 }; // ractSyst
        var p3 = new byte[] { 0x65, 0x6D, 0x2D, 0x4C, 0x69, 0x63, 0x4B, 0x65 }; // em-LicKe
        var p4 = new byte[] { 0x79, 0x2D, 0x32, 0x30, 0x32, 0x36, 0x21, 0x40 }; // y-2026!@
        var key = new byte[32];
        Buffer.BlockCopy(p1, 0, key, 0, 8);
        Buffer.BlockCopy(p2, 0, key, 8, 8);
        Buffer.BlockCopy(p3, 0, key, 16, 8);
        Buffer.BlockCopy(p4, 0, key, 24, 8);
        return key;
    }

    /// <summary>
    /// Calcula el fingerprint de la base de datos.
    /// </summary>
    public static string ComputeFingerprint(string serviceBrokerGuid, string createDate)
    {
        var input = $"{serviceBrokerGuid}|{createDate}";
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        // Formato amigable: grupos de 4 hex separados por guión (primeros 16 bytes = 8 grupos)
        var sb = new StringBuilder();
        for (int i = 0; i < 16; i++)
        {
            if (i > 0 && i % 2 == 0) sb.Append('-');
            sb.Append(hash[i].ToString("X2"));
        }
        return sb.ToString(); // e.g. A7F3-B2D1-C8E5-9F04-1122-3344-5566-7788
    }

    /// <summary>
    /// Genera una clave de licencia firmada para un fingerprint y fecha de expiración dados.
    /// </summary>
    public static string GenerarClave(string fingerprint, DateTime fechaExpiracion)
    {
        var payload = $"{fingerprint}|{fechaExpiracion:yyyy-MM-dd}";
        var payloadBytes = Encoding.UTF8.GetBytes(payload);
        var signature = HMACSHA256.HashData(GetSecretKey(), payloadBytes);

        // Clave = Base64(payload) + "." + Base64(signature)
        var claveRaw = Convert.ToBase64String(payloadBytes) + "." + Convert.ToBase64String(signature);

        // Formato amigable: grupos de 5 chars separados por guión
        return FormatKey(claveRaw);
    }

    /// <summary>
    /// Valida una clave de licencia contra un fingerprint.
    /// Retorna (esValida, fechaExpiracion).
    /// </summary>
    public static (bool EsValida, DateTime FechaExpiracion) ValidarClave(string clave, string fingerprint)
    {
        try
        {
            var claveRaw = UnformatKey(clave);
            var parts = claveRaw.Split('.');
            if (parts.Length != 2) return (false, default);

            var payloadBytes = Convert.FromBase64String(parts[0]);
            var signatureRecibida = Convert.FromBase64String(parts[1]);

            // Verificar firma
            var signatureEsperada = HMACSHA256.HashData(GetSecretKey(), payloadBytes);
            if (!CryptographicOperations.FixedTimeEquals(signatureRecibida, signatureEsperada))
                return (false, default);

            // Descomponer payload
            var payload = Encoding.UTF8.GetString(payloadBytes);
            var payloadParts = payload.Split('|');
            if (payloadParts.Length != 2) return (false, default);

            var fingerprintEnClave = payloadParts[0];
            if (!string.Equals(fingerprintEnClave, fingerprint, StringComparison.OrdinalIgnoreCase))
                return (false, default);

            if (!DateTime.TryParse(payloadParts[1], out var fechaExp))
                return (false, default);

            return (true, fechaExp);
        }
        catch
        {
            return (false, default);
        }
    }

    private static string FormatKey(string raw)
    {
        // Quitar caracteres problemáticos y formatear en grupos de 5
        var clean = raw.Replace("=", "").Replace("+", "-").Replace("/", "_");
        var sb = new StringBuilder();
        for (int i = 0; i < clean.Length; i++)
        {
            if (i > 0 && i % 5 == 0) sb.Append('-');
            sb.Append(clean[i]);
        }
        return sb.ToString();
    }

    private static string UnformatKey(string formatted)
    {
        var clean = formatted.Replace("-", "").Replace(" ", "");
        // Restaurar base64 chars
        clean = clean.Replace("_", "/");
        // Re-add padding if needed
        var raw = clean;
        // Find the dot separator
        var dotIdx = raw.IndexOf('.');
        if (dotIdx < 0) return raw;

        var part1 = PadBase64(raw[..dotIdx]);
        var part2 = PadBase64(raw[(dotIdx + 1)..]);
        return part1 + "." + part2;
    }

    private static string PadBase64(string s)
    {
        var mod = s.Length % 4;
        if (mod > 0) s += new string('=', 4 - mod);
        return s;
    }
}
