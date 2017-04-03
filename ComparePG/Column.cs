using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComparePG
{
    class Column : ItemComparable
    {
        static string SELECT_QUERY = $@"
SELECT table_name as {Constants.TABLE_NAME}
    , column_name as {Constants.ELEMENT_NAME}
    , data_type as {Constants.ELEMENT_TYPE}
    , is_nullable
    , column_default
    , character_maximum_length
FROM information_schema.columns 
WHERE table_schema = '{Constants.REPLACEMENT_SCHEMA}'
and table_name not in 
    (select table_name 
    from information_schema.tables 
    where table_schema='{Constants.REPLACEMENT_SCHEMA}'
    and table_type='VIEW')
AND is_updatable = 'YES'
ORDER BY table_name, column_name;";
        public Column()
        {
            this.Query = SELECT_QUERY;
            PrepareOptions();
        }

        public override string Key()
        {
            return $"{this.TableName}.{this.Name}".ToLowerInvariant();
        }

        private void PrepareOptions()
        {
            OptionValueList.Add(new OptionValue
            {
                OptionName = "is_nullable",
                ComparisonValue = "NO",
                ValueIfEqual = "NOT NULL"
            });

        }
        internal override string Create(InterItemCommunication communication)
        {
            string create = "";

            if (ElementType == "ARRAY")
            {
                create = "--!!! ARRAY : Double check !!!";
            }
            if (DefautValue.EndsWith($".{TableName}_{Name}_seq'::regclass)")){
                ElementType = "SERIAL";
            }

            create += $"ALTER TABLE {TableName} ADD COLUMN {Name} {TypeStringFromElementType} {GenerateOptions()}";

            return create;
        }
        internal override string GenerateOptions()
        {
            string options = base.GenerateOptions();
            if (DefautValue != "null" && ElementType!="SERIAL")
            {
                options += $" DEFAULT {DefautValue}";
            }
            return options;
        }
        internal override string Drop(InterItemCommunication communication)
        {
            if (!communication.TableToDeleteList.Contains(TableName))
            {
                return $"ALTER TABLE {TableName} DROP COLUMN IF EXISTS {Name}";
            }
            //else : no need to remove a column from a table that will be deleted!
            return string.Empty;
        }
        private string DefautValue { get { return this["column_default"]; } }
        private int MaximumLength { get { return Convert.ToInt32(this["character_maximum_length"]); } }
        internal override string Alter(ItemComparable target, InterItemCommunication communication)
        {
            StringBuilder alter = new StringBuilder();
            AddTypeModification(alter, target);
            AddDefaultModification(alter, target);
            return alter.ToString();
        }

        private void AddDefaultModification(StringBuilder alter, ItemComparable target)
        {
            var targetGoodType = target as Column;
            if (targetGoodType.DefautValue == "null" && DefautValue != "null")
            {
                alter.AppendLine($"ALTER TABLE {TableName} ALTER COLUMN {Name} DROP DEFAULT");
            }
            else if (targetGoodType.DefautValue != DefautValue)
            {
                alter.AppendLine($"ALTER TABLE {TableName} ALTER COLUMN {Name} SET DEFAULT {DefautValue}");
            }
        }

        private void AddTypeModification(StringBuilder alter, ItemComparable target)
        {
            var targetGoodType = target as Column;
            if (ElementType == target.ElementType)
            {
                if (ElementType == "character varying" && MaximumLength != targetGoodType.MaximumLength)
                {
                    if (MaximumLength < targetGoodType.MaximumLength)
                    {
                        alter.AppendLine("--!!! Truncation will occur !!!");
                    }
                    alter.AppendLine($"ALTER TABLE {TableName} ALTER COLUMN {Name} TYPE {TypeStringFromElementType}");
                }
                //ELSE ???
            }
            else
            {
                alter.AppendLine($"ALTER TABLE {TableName} ALTER COLUMN {Name} TYPE {TypeStringFromElementType}");
            }
        }

        private string TypeStringFromElementType
        {
            get
            {
                if (ElementType == "character varying")
                {
                    return $"{ElementType} ({MaximumLength})";
                }
                else
                {
                    return $"{ElementType}";
                }
            }
        }
    }
}
