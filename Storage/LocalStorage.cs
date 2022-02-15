﻿using Account_Manager.MVVM.Model;
using Account_Manager.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static Account_Manager.Consts;

namespace Account_Manager.Storage
{
    public class LocalStorage
    {
        private CryptoService _CryptoService;

        public LocalStorage(CryptoService cryptoService)
        {
            _CryptoService = cryptoService;
        }

        private enum FileWriteType { Write, Append, Clear }

        public bool SetAuthData(string UserKey, string HashedPassword)
        {
            try
            {
                AuthModel appAuthModel = new AuthModel
                {
                    UserKey = UserKey,
                    HashedPassword = HashedPassword
                };

                return SetData(DataType.AUTH, JsonSerializer.Serialize(appAuthModel), FileWriteType.Write);
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        public static AuthModel GetAuthData()
        {
            try
            {
                string FileData = File.ReadAllText(GetFilePathFromDataType(DataType.AUTH));
                AuthModel? authModel =  JsonSerializer.Deserialize<AuthModel>(FileData);
                if (authModel == null)
                    throw new NullReferenceException();
                return authModel;
            }
            catch (Exception ex)
            {
                return new AuthModel();
            }
        }

        public bool CreateLocalData<T>(string _DataType, T NewData)
        {
            try
            {
                // Pull Data List
                List<T>? DataList = JsonSerializer.Deserialize<List<T>>(GetData(_DataType));

                // Add New Data
                if (DataList != null)
                    DataList.Add(NewData);
                else
                    throw new NullReferenceException();

                // Put Back Data List
                return SetData(_DataType, JsonSerializer.Serialize(DataList), FileWriteType.Write);
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        public List<T> ReadLocalData<T>(string _DataType)
        {
            try
            {
                List<T>? Data = JsonSerializer.Deserialize<List<T>>(GetData(_DataType));

                if(Data != null)
                    return Data;
                throw new NullReferenceException();
            }
            catch (Exception ex)
            {
                return new List<T>();
            }
        }
        public bool UpdateLocalData<T>(string _DataType, T ReferenceData, T UpdatedData) where T : ModelBase
        {
            try
            {
                // Pull Data List
                List<T>? DataList = JsonSerializer.Deserialize<List<T>>(GetData(_DataType));

                if (DataList != null)
                {
                    for (int i = 0; i < DataList.Count; i++)
                    {
                        // Find Reference Data
                        if (DataList[i].Equals(ReferenceData))
                        {
                            // Change Data
                            DataList[i] = UpdatedData;
                            break;
                        }
                    }
                }
                else
                    throw new NullReferenceException();

                return SetData(_DataType, JsonSerializer.Serialize(DataList), FileWriteType.Write);
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        public bool DeleteLocalData<T>(string _DataType, T ReferenceData) where T : ModelBase
        {
            try
            {
                // Pull Data List
                List<T>? DataList = JsonSerializer.Deserialize<List<T>>(GetData(_DataType));

                if (DataList != null)
                {
                    for (int i = 0; i < DataList.Count; i++)
                    {
                        // Find Reference Data
                        if (DataList[i].Equals(ReferenceData))
                        {
                            // Delete Data
                            DataList.RemoveAt(i);
                            break;
                        }
                    }
                }
                else
                    throw new NullReferenceException();

                return SetData(_DataType, JsonSerializer.Serialize(DataList), FileWriteType.Write);
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private static string GetFilePathFromDataType(string _DataType)
        {
            switch (_DataType)
            {
                case DataType.ACCOUNT:
                    return Files.ACCOUNTS_PATH;
                case DataType.SITE:
                    return Files.SITES_PATH;
                case DataType.AUTH:
                    return Files.AUTHENTICATION_PATH;
            }
            return "";
        }

        private bool SetData(string _Type, string _Data, FileWriteType _FileWriteType)
        {
            try
            {
                if (!_Type.Equals(DataType.AUTH))
                    _Data = _CryptoService.Encrypt(_Data);

                switch (_FileWriteType)
                {
                    case FileWriteType.Append:
                        File.AppendAllText(GetFilePathFromDataType(_Type), _Data);
                        break;
                    case FileWriteType.Write:
                        File.WriteAllText(GetFilePathFromDataType(_Type), _Data);
                        break;
                    case FileWriteType.Clear:
                        File.WriteAllText(GetFilePathFromDataType(_Type), "");
                        break;
                }

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private string GetData(string _Type)
        {
            try
            {
                string FileData = File.ReadAllText(GetFilePathFromDataType(_Type));
                FileData = _CryptoService.Decrypt(FileData);
                return FileData;
            }
            catch (Exception ex)
            {
                return "";
            }
        }
    }
}
