using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Markup;

namespace DGJv3
{
    public class EnumerationExtension : MarkupExtension
    {
        private Type _enumType;

        public EnumerationExtension(Type enumType) => EnumType = enumType ?? throw new ArgumentNullException("enumType");
        public override object ProvideValue(IServiceProvider serviceProvider) => (from object enumValue in Enum.GetValues(EnumType) select new EnumerationMember { Value = enumValue, Description = GetDescription(enumValue) }).ToArray();
        private string GetDescription(object enumValue) => EnumType.GetField(enumValue.ToString()).GetCustomAttributes(typeof(DescriptionAttribute), false).FirstOrDefault() is DescriptionAttribute descriptionAttribute ? descriptionAttribute.Description : enumValue.ToString();

        public Type EnumType
        {
            get { return _enumType; }
            private set
            {
                if (_enumType == value)
                    return;

                var enumType = Nullable.GetUnderlyingType(value) ?? value;

                if (enumType.IsEnum == false)
                    throw new ArgumentException("Type must be an Enum.");

                _enumType = value;
            }
        }

        public class EnumerationMember
        {
            public string Description { get; set; }
            public object Value { get; set; }
        }
    }
}
