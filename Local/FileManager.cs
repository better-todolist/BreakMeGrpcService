using BreakMeGrpcService.DataObj;
using System.Text.Json;

namespace BreakMeGrpcService.Local
{
    public class FileManager
    {
        // 项目基数据文件夹
        private static readonly string BaseDir = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "break_me");
        private static readonly string DataDir = Path.Join(BaseDir, "data");
        private static readonly string ConfigFile = Path.Join(BaseDir, "cfg.json");
        private static readonly Mutex InitGruad = new(false);
        private static bool Initialized = false;

        public static void init()
        {

            InitGruad.WaitOne();
            try
            {
                if (!Initialized)
                {
                    if (!Directory.Exists(BaseDir))
                    {
                        Directory.CreateDirectory(BaseDir);
                    }

                    if (!File.Exists(ConfigFile))
                    {
                        using FileStream sw = File.OpenWrite(ConfigFile);
                        var cfg = LocalConfig.get_default();
                        var bytes = JsonSerializer.SerializeToUtf8Bytes<LocalConfig>(cfg);
                        sw.Write(bytes);
                    }
                    if (!Directory.Exists(DataDir))
                    {
                        Directory.CreateDirectory(DataDir);
                    }
                    Initialized = true;

                }

            }
            finally { InitGruad.ReleaseMutex(); }

        }

        public static async Task<LocalConfig> GetConfig()
        {
            init();
            using FileStream file = File.OpenRead(ConfigFile);
            LocalConfig? localConfig = await JsonSerializer.DeserializeAsync<LocalConfig>(file);
            return localConfig ?? LocalConfig.get_default();
        }

        public static async void updateConfig(LocalConfig config)
        {
            init();
            using FileStream file = File.OpenWrite(ConfigFile);
            await JsonSerializer.SerializeAsync(file, config);
        }

        public static async Task<IList<IntpData>> GetAllIntpTask()
        {
            init();
            var list = Directory.GetFiles(DataDir);
            var task_list = new List<IntpData>();

            foreach (var file in list)
            {
                using FileStream stream = File.OpenRead(file);
                IntpData? intpData = await JsonSerializer.DeserializeAsync<IntpData>(stream);
                if (intpData != null)
                {
                    task_list.Add(intpData);
                }
            }
            return task_list;
        }

        public static async Task<IntpData?> GetIntpInfo(Guid id)
        {
            init();
            var path = Path.Join(DataDir, $"{id}.json");
            if (File.Exists(path))
            {
                using FileStream file = File.OpenRead(path);
                IntpData? data = await JsonSerializer.DeserializeAsync<IntpData>(file);

                return data;
            }
            else
            {
                return null;
            }
        }

        public static async Task<Guid?> CreateIntp(IntpData data)
        {
            init();
            var path = Path.Join(DataDir, $"{data.Id}.json");
            using FileStream file = File.Create(path);
            await JsonSerializer.SerializeAsync(file, data);

            return data.Id;
        }

        public static void RemoveIntp(Guid id)
        {
            init();
            var path = Path.Join(DataDir, $"{id}.json");
            File.Delete(path);
        }


    }
}
