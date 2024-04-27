This folder is used to store the following game dependencies:

- Lidgren.Network.dll
- Lidgren.Network.pdb
- Microsoft.Xna.Framework.dll
- Microsoft.Xna.Framework.Game.dll
- Microsoft.Xna.Framework.Graphics.dll
- Microsoft.Xna.Framework.Xact.dll
- Secrets of Grindea.exe
- Secrets of Grindea.pdb
- Steamworks.NET.dll

Either find them and put them in yourself, or run the dependency finder script from the Utils folder:

```
python Utils/utils.py fetch-deps
```

In addition, you will need to include the ModBagman executable assembly, specifically the **merged** version.
