# ProyectoDAM (Tetraversus)

Este repositorio contiene el proyecto **Tetraversus**, desarrollado con **Unity** y una **REST API** en local.

## Requisitos

- **Unity**: versión **6000.3.8f1**
- **XAMPP** (o equivalente) con:
  - **MariaDB**
  - **phpMyAdmin**
- Un **IDE** (en mi caso, **JetBrains Rider**)

## Puesta en marcha

1. **Configurar el puerto en Unity**
   - Edita el archivo de configuración ubicado en:
     - `ProyectoDAM/TetraversusGit/Assets/StreamingAssets`
   - Ajusta el **puerto** para que coincida con el que utiliza tu REST API.

2. **Localhost**
   - El host `localhost` ya está configurado.
   - Solo es necesario **cambiar el puerto**.

3. **Ejecutar la REST API y obtener el certificado autofirmado**
   - Inicia primero la **REST API**.
   - Después ejecuta:
     - `ProyectoDAM/GetSSLcertified`
   - Este programa obtendrá el **pin SHA-256** del certificado autofirmado (certificate pin).
   - Copia ese valor y pégalo en el **JSON** del paso 1 (configuración de Unity).

4. **Arrancar el juego**
   - Abre el proyecto en Unity, ejecuta la escena principal y ¡a jugar!