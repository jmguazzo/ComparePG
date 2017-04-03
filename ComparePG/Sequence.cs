
namespace ComparePG
{
    class Sequence : ItemComparable
    {

        //We select all sequences except the one from SERIAL
        static string SELECT_QUERY = $@"SELECT 
sequence_name as {Constants.ELEMENT_NAME}
,'SEQUENCE' as {Constants.ELEMENT_TYPE}
, data_type
, start_value
, minimum_value
, maximum_value
, increment
, cycle_option 
FROM information_schema.sequences
WHERE sequence_schema ='{Constants.REPLACEMENT_SCHEMA}'
and not exists (
select 1 from information_Schema.columns where 
	lower(column_default)=lower('nextval('''||sequence_schema||'.'||sequence_name||'''::regclass)')
	or
	lower(column_default)=lower('nextval(''""'||sequence_schema||'"".""'||sequence_name||'""''::regclass)')
    or
    lower(column_default)=lower('nextval('''||sequence_schema||'.""'||sequence_name||'""''::regclass)')
)
ORDER BY sequence_name;";
        public Sequence()
        {
            this.Query = SELECT_QUERY;
            this.IfExists = true;
        }
        internal override string Create(InterItemCommunication communication)
        {
            return $"CREATE {ElementType} {Name} INCREMENT {this["increment"]} MINVALUE {this["minimum_value"]} MAXVALUE {this["maximum_value"]} START {this["start_value"]};";
        }

        internal override string Alter(ItemComparable target, InterItemCommunication communication)
        {
            return string.Empty;
        }
    }
}
