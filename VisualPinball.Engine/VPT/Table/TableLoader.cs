using System;
using System.IO;
using OpenMcdf;

namespace VisualPinball.Engine.VPT.Table
{
	/// <summary>
	/// The entry point for loading and parsing the VPX file.
	/// </summary>
	public static class TableLoader
	{
		private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

		public static Table Load(string filename)
		{
			var cf = new CompoundFile(filename);
			try {
				var gameStorage = cf.RootStorage.GetStorage("GameStg");
				var gameData = gameStorage.GetStream("GameData");
				var bytes = gameData.GetData();

				using (var stream = new MemoryStream(bytes))
				using (var reader = new BinaryReader(stream)) {
					var table = new Table(reader);

					LoadGameItems(table, gameStorage);
					LoadTextures(table, gameStorage);

					// print some random data
					Logger.Debug("left = {0}", table.Data.Left);
					Logger.Debug("BgRotation = {0}", string.Join("/", table.Data.BgRotation));
					Logger.Debug("name = {0}", table.Data.Name);

					return table;
				}

			} finally {
				cf.Close();
			}
		}

		private static void LoadGameItems(Table table, CFStorage storage)
		{
			for (var i = 0; i < table.Data.NumGameItems; i++) {
				var itemName = $"GameItem{i}";
				var itemStream = storage.GetStream(itemName);
				var itemData = itemStream.GetData();
				if (itemData.Length < 4) {
					Logger.Warn("Skipping {itemName} because it has size of {itemDataLength}.", itemName, itemData.Length);
					continue;
				}

				var reader = new BinaryReader(new MemoryStream(itemData));
				var itemType = reader.ReadInt32();
				switch (itemType) {
					case ItemType.Primitive: {
						Logger.Info("Loading primitive {itemName}", itemName);
						var item = new VisualPinball.Engine.VPT.Primitive.Primitive(reader, itemName);
						table.Primitives[item.Name] = item;
						break;
					}
				}
			}
		}

		private static void LoadTextures(Table table, CFStorage storage)
		{
			for (var i = 0; i < table.Data.NumTextures; i++) {
				var textureName = $"Image{i}";
				var textureStream = storage.GetStream(textureName);
				var textureData = textureStream.GetData();
				if (textureData.Length < 4) {
					Logger.Warn("Skipping {itemName} because it has size of {itemDataLength}.", textureName, textureData.Length);
					continue;
				}

				var reader = new BinaryReader(new MemoryStream(textureData));
				Logger.Info("Loading texture {itemName}", textureName);
				var texture = new Texture(reader, textureName);
				table.Textures[texture.Name] = texture;
			}
		}
	}
}
