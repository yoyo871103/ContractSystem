# Convención: Diálogos (ventanas modales)

## Estructura general

Los diálogos deben seguir esta estructura de filas para que el scroll solo afecte al cuerpo y los botones de acción permanezcan fijos en la parte inferior:

1. **Fila 0 (Auto)**: Título/encabezado fijo (barra con título y botón cerrar).
2. **Fila 1 (*)**: Cuerpo con **ScrollViewer**; dentro, el contenido que puede crecer (formularios, listas, etc.).
3. **Fila 2 (Auto)**: Pie fijo con botones de acción (Cancelar, Guardar, Aceptar, etc.).

Ejemplo en XAML:

```xml
<Grid>
    <Grid.RowDefinitions>
        <RowDefinition Height="Auto"/>   <!-- Título -->
        <RowDefinition Height="*"/>      <!-- Cuerpo con scroll -->
        <RowDefinition Height="Auto"/>   <!-- Pie fijo -->
    </Grid.RowDefinitions>
    <!-- Título (fijo) -->
    <Border Grid.Row="0" ...> ... </Border>
    <!-- Cuerpo (con scroll) -->
    <ScrollViewer Grid.Row="1" VerticalScrollBarVisibility="Auto" Margin="20,16">
        <StackPanel> ... contenido ... </StackPanel>
    </ScrollViewer>
    <!-- Pie fijo: botones -->
    <Border Grid.Row="2" Background="White" BorderBrush="..." BorderThickness="0,1,0,0" Padding="20,12">
        <StackPanel Orientation="Horizontal" HorizontalAlignment="Right">
            <Button Content="Cancelar" .../>
            <Button Content="Guardar" .../>
        </StackPanel>
    </Border>
</Grid>
```

Diálogos que ya siguen esta convención: **EditProfileWindow**, **ChangePasswordWindow**, **ResetPasswordWindow**, **ConnectionConfigWindow**.
