using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;

namespace ComparePG
{
    class Program
    {
        static InterItemCommunication Communication = new InterItemCommunication();
        private static Parameter parameter;
        private static Validation validation = new Validation();
        static void Main(string[] args)
        {
            parameter = new Parameter(args);
            if (!parameter.Complete)
            {
                Console.WriteLine(parameter.Usage());
                return;
            }
            var sourceDb = CreateDb(parameter.Source);
            var targetDb = CreateDb(parameter.Target);
            if(!validation.CheckDb(sourceDb) || !validation.CheckDb(targetDb))
            {
                Console.WriteLine(validation.Usage());
                Console.WriteLine(parameter.Usage());
                return;

            }


            DisplayInstruction($"--Start of script");

            DisplayInstruction($"set search_path='{targetDb.Schema}';");

            if (parameter.ItemGroupRole) Compare<GroupRole>(sourceDb, targetDb);
            if (parameter.ItemSchema) Compare<Schema>(sourceDb, targetDb);
            if (parameter.ItemSequence) Compare<Sequence>(sourceDb, targetDb);
            if (parameter.ItemTable) Compare<Table>(sourceDb, targetDb);
            if (parameter.ItemColumn) Compare<Column>(sourceDb, targetDb);
            if (parameter.ItemIndex) Compare<Index>(sourceDb, targetDb);
            if (parameter.ItemView) Compare<View>(sourceDb, targetDb);
            if (parameter.ItemComment) Compare<Comment>(sourceDb, targetDb);
            if (parameter.ItemFunction) Compare<Function>(sourceDb, targetDb);
            if (parameter.ItemForeignKey) Compare<ForeignKey>(sourceDb, targetDb);
            if (parameter.ItemOwner) Compare<Owner>(sourceDb, targetDb);
            if (parameter.ItemGrant) Compare<Grant>(sourceDb, targetDb);
            if (parameter.ItemTrigger) Compare<Trigger>(sourceDb, targetDb);

            DisplayInstruction($"--End of script");

        }

        private static Database CreateDb(Dictionary<string, string> paramsList)
        {
            Database db = new Database();
            db.Name = paramsList[Parameter.DB];
            db.Server = paramsList[Parameter.SERVER];
            db.Login = paramsList[Parameter.LOGIN];
            db.Port = paramsList[Parameter.PORT];
            db.Schema = paramsList[Parameter.SCHEMA];
            db.Password = paramsList[Parameter.PASSWORD];
            return db;
        }


        private static void Compare<T>(Database sourceDb, Database targetDb) where T : ItemComparable, new()
        {
            T item = new T();

            List<T> sourceList = ReturnItemList<T>(sourceDb, item.Query);
            List<T> targetList = ReturnItemList<T>(targetDb, item.Query);

            CompareItemToCreate(sourceList, targetList);

            CompareItemToDrop(sourceList, targetList);

            CompareItemToAlter(sourceList, targetList);

        }

        private static void CompareItemToAlter<T>(List<T> sourceList, List<T> targetList) where T : ItemComparable, new()
        {
            foreach (var t in sourceList)
            {
                var tCible = targetList.FirstOrDefault(y => y.Key() == t.Key());
                if (tCible != null && t != tCible)
                {
                    DisplayInstruction(t.Alter(tCible, Communication));
                }
            }

        }

        private static void CompareItemToDrop<T>(List<T> sourceList, List<T> targetList) where T : ItemComparable, new()
        {
            GetMissingItem(targetList,sourceList)
                .ForEach(x =>
                {
                    DisplayInstruction(x.Drop(Communication));
                    AddDropCommunication(x);
                });
        }
        private static List<T> GetMissingItem<T>(List<T> itemList, List<T> listToSearch) where T : ItemComparable, new()
        {
            return itemList
                .Where(x => MissingItem(x, listToSearch))
                .ToList();


        }
        private static bool MissingItem<T>(ItemComparable x, List<T> list) where T : ItemComparable, new()
        {
            return !list.Any(y => y.Key() == x.Key());
        }

        private static void CompareItemToCreate<T>(List<T> sourceList, List<T> targetList) where T : ItemComparable, new()
        {
            GetMissingItem(sourceList,targetList)
                .ForEach(x =>
                {
                    DisplayInstruction(x.Create(Communication));
                    AddCreateCommunication(x);
                });
        }

        private static void AddCreateCommunication<T>(T addedItem) where T : ItemComparable, new()
        {
            if (addedItem is Table || addedItem is View)
            {
                Communication.TableToDeleteList.Add(addedItem.Name);
            }
        }

        private static void AddDropCommunication<T>(T droppedItem) where T : ItemComparable, new()
        {
            if (droppedItem is Table || droppedItem is View)
            {
                Communication.TableToDeleteList.Add(droppedItem.Name);
            }
        }

        private static void DisplayInstruction(string instruction)
        {
            if (!string.IsNullOrEmpty(instruction))
            {
                if (!instruction.EndsWith(";"))
                    instruction += ";";
                Console.WriteLine("{0}", instruction);
            }
        }

        private static List<T> ReturnItemList<T>(Database db, string query) where T : ItemComparable, new()
        {
            List<T> list = new List<T>();
            query = query.Replace(Constants.REPLACEMENT_SCHEMA, db.Schema);
            using (var con = new Npgsql.NpgsqlConnection(db.ConnectionString()))
            {
                using (var command = con.CreateCommand())
                {
                    command.CommandText = query;

                    con.Open();

                    DbDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        T item = new T();
                        item.Fill(reader);
                        list.Add(item);
                    }
                }
            }

            return list;
        }

    }
}

