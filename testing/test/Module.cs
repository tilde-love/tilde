using System;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Tilde.Module;

namespace App
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum TestEnum
    {
        One,
        Two,
        Three,
        Duck,
        Goose,
        Pig
    }

    public class Module : IRunnable
    {
        private readonly BoolControl bool1 = new BoolControl("bool1", false);
        private readonly EnumControl<TestEnum> enum1 = new EnumControl<TestEnum>("enum1", false);
        private readonly FloatControl float1 = new FloatControl("float1", false, -10, 10, 0.1f);
        private readonly IntControl int1 = new IntControl("int1", false, 0, 10);
        private readonly StringControl string1 = new StringControl("string1", false);

        public void Pause()
        {
        }

        public void Play()
        {
        }

        // Run the actual module code 
        public async Task Run(ModuleConnection moduleConnection, CancellationToken cancellationToken)
        {
            try
            {
                Console.WriteLine("Module start");

                await bool1.Add(moduleConnection);
                await int1.Add(moduleConnection);
                await float1.Add(moduleConnection);
                await string1.Add(moduleConnection);
                await enum1.Add(moduleConnection);

                float[] array = new float[8];

                Random rand = new Random();

                int i = 0;

                bool boolValue = false;
                bool1.Value = boolValue;

                int intValue = 0;
                int1.Value = intValue;

                float floatValue = 0;
                float1.Value = floatValue;

                string stringValue = "";
                string1.Value = stringValue;

                TestEnum enumValue = TestEnum.Duck;
                enum1.Value = enumValue;

                while (cancellationToken.IsCancellationRequested == false)
                {
                    for (int j = 0; j < array.Length; j++)
                    {
                        array[j] = (float) rand.NextDouble();
                    }

                    if (bool1.Value != boolValue)
                    {
                        boolValue = bool1.Value;
                        Console.WriteLine($"Value changed {nameof(bool1)} " + boolValue);
                    }

                    if (int1.Value != intValue)
                    {
                        intValue = int1.Value;
                        Console.WriteLine($"Value changed {nameof(int1)} " + intValue);
                    }

                    if (float1.Value != floatValue)
                    {
                        floatValue = float1.Value;
                        Console.WriteLine($"Value changed {nameof(float1)} " + floatValue);
                    }

                    if (string1.Value != stringValue)
                    {
                        stringValue = string1.Value;
                        Console.WriteLine($"Value changed {nameof(string1)} " + stringValue);
                    }

                    if (enum1.Value != enumValue)
                    {
                        enumValue = enum1.Value;
                        Console.WriteLine($"Value changed {nameof(enum1)} " + enumValue);
                    }

                    bool1.Value = i++ % 2 == 0;
                    float1.Value = (float) rand.NextDouble() * 20f - 10f;
//                moop2.Value = i;
//                moop3.Value = $"MOOP {i}";
//                moop4.Value = array;
//                
                    await Task.Delay(100);
                }
            }
            catch (TaskCanceledException ex)
            {
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            finally
            {
                await bool1.Remove();
                await int1.Remove();
                await float1.Remove();
                await string1.Remove();
                await enum1.Remove();

                Console.WriteLine("Module end");
            }
        }
    }
}