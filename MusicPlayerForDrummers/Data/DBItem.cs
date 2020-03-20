using System;
using System.Collections.Generic;
using System.Text;

namespace MusicPlayerForDrummers.Data
{
    //Represents a generic row from an Sql Table
    public abstract class DBItem
    {
        public string TableName;

        private SqlProperty[] Properties;

        public string[] Values;

        private SqlProperty ID;

        protected DBItem(string tableName)
        {
            this.TableName = tableName;
            this.ID = new SqlProperty(tableName + "ID", EType.INT, true);
        }

        //TODO: Or the opposite, array of all properties and remove the first one when needed?
        //Returns all the properties except the ID
        public SqlProperty[] getCustomProperties()
        {
            return Properties;
        }

        //Returns all the properties, including the ID
        public SqlProperty[] getAllProperties()
        {
            SqlProperty[] allProps = new SqlProperty[Properties.Length+1];
            allProps[0] = ID;
            Array.Copy(Properties, 0, allProps, 1, Properties.Length);
            return allProps;
        }

        public void setCustomProperties(params SqlProperty[] properties)
        {
            Properties = properties;
        }
    }
}
