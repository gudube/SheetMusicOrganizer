These files come directly from NAudio Github (https://github.com/naudio/NAudio) v2.0.0
They are part of the NAudio dependency on Nuget, but this dependency also includes stuff like NAudio.WinForms, which cause problems when using it with this WPF projet.
Simply adding the needed dependencies on Nuget (Core, Wasapi, WinMM...) and adding those two files manually solve the problem.
Update files if needed.