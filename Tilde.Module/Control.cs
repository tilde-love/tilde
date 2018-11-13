using System;
using System.Threading.Tasks;
using Tilde.SharedTypes;

namespace Tilde.Module
{
    public abstract class Control
    {
        private ModuleConnection connection;
        
        protected readonly bool Readonly;
        protected readonly DataSourceType SourceType;
        protected readonly Uri Uri;
        protected readonly NumericRange? Range;
        protected readonly string[] Values;
        protected readonly Graph Graph;

        public Control(
            Uri uri,
            DataSourceType sourceType,
            bool @readonly, 
            NumericRange? range,
            string[] values,
            Graph graph)
        {
            Uri = uri;
            SourceType = sourceType;
            Readonly = @readonly;
            Range = range;
            Values = values;
            Graph = graph;
        }

        public async Task Add(ModuleConnection connection)
        {
            this.connection = connection;
            
            await connection.DefineDataSource(Uri, SourceType, Readonly, Range, Values, Graph);

            if (Readonly == false)
            {
                connection.RegisterValueChange(Uri, ValueUpdated);
            }
        }

        public async Task Remove()
        {
            if (Readonly == false)
            {
                connection.UnregisterValueChange(Uri);
            }

            await connection.DeleteDataSource(Uri);
        }

        protected async Task SetValue(string connectionId, object value)
        {
            await connection.SetValue(Uri, connectionId, value);
        }

        protected abstract void ValueUpdated(Uri uri, string connectionId, object value);
    }

    public abstract class Control<T> : Control
    {
        protected T value;

        public T Value
        {
            get => value;
            set
            {
                this.value = value;

                SetValue(null, this.value)
                    .Wait();
            }
        }

        protected Control(
            string uri,
            DataSourceType dataSourceType,
            bool @readonly = false,
            NumericRange? range = null,
            string[] values = null,
            Graph graph = null)
            : base(new Uri(uri, UriKind.RelativeOrAbsolute), dataSourceType, @readonly, range, values, graph)
        {
        }
    }

    public class BoolControl : Control<bool>
    {
        public BoolControl(string uri, bool @readonly)
            : base(
                uri,
                DataSourceType.Boolean,
                @readonly,
                null,
                new string[]
                {
                    "false", "true"
                }
            )
        {
        }

        protected override async void ValueUpdated(Uri uri, string connectionId, object value)
        {
            if (Readonly == true)
            {
                return; 
            }

            switch (value)
            {
                case bool b:
                    this.value = b;
                    break;
                case string s:
                    bool.TryParse(s, out this.value);
                    break;
            }
            
            await SetValue(connectionId, this.value);
        }
    }

    public class IntControl : Control<int>
    {
        public IntControl(string uri, bool @readonly, int min, int max)
            : base(uri, DataSourceType.Integer, @readonly, new NumericRange { Maximum = max, Minimum = min, Step = 1 }, null)
        {
        }

        protected override async void ValueUpdated(Uri uri, string connectionId, object value)
        {
            if (Readonly == true)
            {
                return; 
            }

            switch (value)
            {
                case int i:
                    this.value = i;
                    break;
                case long l:
                    this.value = (int)l;
                    break;
                case float f:
                    this.value = (int)f;
                    break;
                case double d:
                    this.value = (int)d;
                    break;
                case string s:
                    int.TryParse(s, out this.value);
                    break;
            }
            
            await SetValue(connectionId, this.value);
        }
    }
    
    public class FloatControl : Control<float>
    {
        public FloatControl(string uri, bool @readonly, float min, float max, float step)
            : base(
                uri,
                DataSourceType.Float,
                @readonly,
                new NumericRange
                {
                    Maximum = max,
                    Minimum = min,
                    Step = step
                },
                null
            )
        {
        }

        protected override async void ValueUpdated(Uri uri, string connectionId, object value)
        {
            if (Readonly == true)
            {
                return; 
            }

            switch (value)
            {
                case float f:
                    this.value = (float)f;
                    break;
                case double d:
                    this.value = (float)d;
                    break;
                case int i:
                    this.value = (float)i;
                    break;
                case long l:
                    this.value = (float)l;
                    break;
                case string s:
                    float.TryParse(s, out this.value);
                    break;
            }
            
            await SetValue(connectionId, this.value);
        }
    }

    public class StringControl : Control<string>
    {
        public StringControl(string uri, bool @readonly)
            : base(uri, DataSourceType.String, @readonly, null, null)
        {
        }

        protected override async void ValueUpdated(Uri uri, string connectionId, object value)
        {
            if (Readonly == true)
            {
                return; 
            }

            switch (value)
            {
                case string s:
                    this.value = s; 
                    break;
                default:
                    this.value = base.value.ToString();
                    break; 
            }
            
            await SetValue(connectionId, this.value);
        }
    }
    
    public class EnumControl<TEnum> : Control<TEnum> where TEnum : struct, IConvertible
    {
        public EnumControl(string uri, bool @readonly)
            : base(uri, DataSourceType.Enum, @readonly, null, Enum.GetNames(typeof(TEnum)))
        {
            if (typeof(TEnum).IsEnum == false) 
            {
                throw new ArgumentException("TEnum must be an enumeration type", nameof(TEnum));
            }
        }

        protected override async void ValueUpdated(Uri uri, string connectionId, object value)
        {
            if (Readonly == true)
            {
                return; 
            }

            switch (value)
            {
                case string s:
                    Enum.TryParse(s, true, out this.value); 
                    break;
                default:
                    break; 
            }
            
            await SetValue(connectionId, this.value);
        }
    }
    
//    public class GraphControl : Control<Graph>
//    {
//        public GraphControl(string uri, bool @readonly)
//            : base(uri, DataSourceType.Graph, @readonly, null, null, ))
//        {
//            if (typeof(TEnum).IsEnum == false) 
//            {
//                throw new ArgumentException("TEnum must be an enumeration type", nameof(TEnum));
//            }
//        }
//
//        protected override async void ValueUpdated(Uri uri, string connectionId, object value)
//        {
//            if (Readonly == true)
//            {
//                return; 
//            }
//
//            switch (value)
//            {
//                case string s:
//                    Enum.TryParse(s, true, out this.value); 
//                    break;
//                default:
//                    break; 
//            }
//            
//            await SetValue(connectionId, this.value);
//        }
//    }
    
//    public class Control<T> : Control
//    {
//        private T value;
//
//        public T Value
//        {
//            get => value;
//            set
//            {
//                this.value = value;
//
//                SetValue(null, this.value)
//                    .Wait();
//            }
//        }
//
//        public Control(Uri uri, bool @readonly)
//            : base(uri, GetDataSourceType(), @readonly)
//        {
//        }
//        
//        public Control(string uri, bool @readonly)
//            : base(new Uri(uri, UriKind.RelativeOrAbsolute), GetDataSourceType(), @readonly)
//        {
//        }
//
//        /// <inheritdoc />
//        protected override async void ValueUpdated(Uri uri, string connectionId, object value)
//        {
//            if (Readonly == true)
//            {
//                return; 
//            }
//
//            switch (value)
//            {
//                case bool b:
//                    if (SourceType == DataSourceType.Boolean)
//                        this.value = (T) value; 
//                    
//                    break;
//                case double d: break;
//            }
//
//            switch (SourceType)
//            {
//                case DataSourceType.Boolean: break;
//                case DataSourceType.Enum: break;
//                case DataSourceType.String: break;
//                case DataSourceType.Float: break;
//                case DataSourceType.FloatArray: break;
//                case DataSourceType.Integer: break;
//                case DataSourceType.IntegerArray: break;
//                case DataSourceType.Color: break;
//                case DataSourceType.Image: break;
//                case DataSourceType.Any: break;
//                default: throw new ArgumentOutOfRangeException();
//            }
//            
//            this.value = (T) value;
//
//            await SetValue(connectionId, value);
//        }
//
//        private static DataSourceType GetDataSourceType()
//        {
//            Type type = typeof(T);
//
//            if (type == typeof(float))
//            {
//                return DataSourceType.Float;
//            }
//
//            if (type == typeof(int))
//            {
//                return DataSourceType.Integer;
//            }
//
//            if (type == typeof(bool))
//            {
//                return DataSourceType.Boolean;
//            }
//
//            if (type == typeof(string))
//            {
//                return DataSourceType.String;
//            }
//
//            if (type == typeof(float[]))
//            {
//                return DataSourceType.FloatArray;
//            }
//
//            if (type == typeof(int[]))
//            {
//                return DataSourceType.IntegerArray;
//            }
//
//            if (type.IsEnum)
//            {
//                return DataSourceType.Enum;
//            }
//
//            return DataSourceType.Any;
//        }
//    }
}