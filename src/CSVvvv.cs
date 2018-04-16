using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Text;

using CsvHelper;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Nodes
{
    [PluginInfo(Name = "Reader", Version = "CSV", Category = "File", Author ="Ro.", AutoEvaluate = true)]
    public class CSVvvv : IPluginEvaluate
    {
        [Input("File", FileMask = "CSV File (*.csv)|*.csv", StringType = StringType.Filename, IsSingle = true, Order = 0, DefaultString = "")]
        public IDiffSpread<string> FilePath;

        [Input("Reload", IsBang = true, IsSingle = true)]
        public IDiffSpread<bool> Reload;

        [Config("Delimiter", IsSingle = true, DefaultString = ";")]
        public ISpread<string> Delimiter;

        [Config("Encoding", IsSingle = true, EnumName = "Encoding", DefaultEnumEntry = "utf-8")]
        public ISpread<EnumEntry> Encoding;

        [Import]
        private IIOFactory IOFactory = null;

        private Dictionary<string, IIOContainer<ISpread<string>>> Outputs = new Dictionary<string, IIOContainer<ISpread<string>>>();

        public CSVvvv() {
            var encodings = System.Text.Encoding.GetEncodings().Select((e) => e.Name).OrderBy(n => n).ToArray();

            EnumManager.UpdateEnum("Encoding", "utf-8", encodings);
        }

        public void Evaluate(int SpreadMax)
        {
            if (string.IsNullOrEmpty(FilePath[0]) || (!FilePath.IsChanged && !Reload[0])) return;

            if (!File.Exists(FilePath[0])) throw new FileNotFoundException("File not found", FilePath[0]);

            var config = new CsvHelper.Configuration.Configuration {
                Delimiter = Delimiter[0],
                Encoding = System.Text.Encoding.GetEncoding(Encoding[0].Name)
            };

            using (var fileStream = File.OpenText(FilePath[0])) {
                using (var csv = new CsvReader(fileStream, config)) {
                    csv.Read();
                    csv.ReadHeader();

                    Outputs.Keys.Except(csv.Context.HeaderRecord).ToList().ForEach(oldHeader => {
                        Outputs[oldHeader].Dispose();
                        Outputs.Remove(oldHeader);
                    });

                    csv.Context.HeaderRecord.Except(Outputs.Keys).ToList().ForEach(newHeader => {
                        var newOutput = new OutputAttribute(newHeader);
                        Outputs.Add(newHeader, IOFactory.CreateIOContainer<ISpread<string>>(newOutput));
                    });

                    for (var i = 0; csv.Read(); i++) {
                        Outputs.ToList().ForEach(Output => {
                            Output.Value.IOObject.ResizeAndDismiss(i+1, (data) => "");
                            Output.Value.IOObject[i] = csv[Output.Key];
                        });
                    }
                }
            }
        }
    }
}
