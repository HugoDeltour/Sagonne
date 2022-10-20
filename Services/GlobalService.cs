using Helpers;
using Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Services
{
    public class GlobalService : IGlobalService
    {
        private readonly Dictionary<string, string> _dbConfigs;

        public string DatabaseServer
        {
            get
            {
                try
                {
                    return _dbConfigs[WebHelpers.GetUserName];
                }
                catch (Exception)
                {

                    _dbConfigs.Add(WebHelpers.GetUserName,"SERVEUR_SQL");
                    return _dbConfigs[WebHelpers.GetUserName];
                }
            }
        }

        public GlobalService()
        {
            _dbConfigs = new Dictionary<string, string>();
        }

        public void SetDatabase(string database)
        {
            if (_dbConfigs.ContainsKey(WebHelpers.GetUserName))
            {
                _dbConfigs[WebHelpers.GetUserName] = database;
            }
            else
            {
                _dbConfigs.Add(WebHelpers.GetUserName, database);
            }
        }
    }
}
