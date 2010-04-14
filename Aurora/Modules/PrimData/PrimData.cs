﻿using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using log4net;
using Nini.Config;
using Aurora.Framework;
using OpenSim.Region.Framework.Interfaces;
using OpenSim.Region.Framework.Scenes;
using Aurora.DataManager;
using OpenSim.Framework;
using OpenMetaverse;
using OpenMetaverse.StructuredData;
using OpenSim.Region.CoreModules.Avatar.NPC;

namespace Aurora.Modules
{
    enum PrimTypes
    {
        Normal = 0,
        Bot = 1,
        Oar = 2
    }
    public class PrimData: IRegionModule
    {
        IRemoteGenericData GenericData;
        Scene m_scene;
        INPCModule BotModule;
        public void Initialise(Scene scene, IConfigSource source)
        {
            m_scene = scene;
            scene.SceneContents.OnObjectDuplicate += new ObjectDuplicateDelegate(SceneContents_OnObjectDuplicate);
            scene.SceneContents.OnObjectCreate += new ObjectCreateDelegate(SceneContents_OnObjectCreate);
            scene.SceneContents.OnObjectRemove += new ObjectDeleteDelegate(SceneContents_OnObjectRemove);
        }

        void SceneContents_OnObjectDuplicate(EntityBase original, EntityBase clone)
        {
            if (GenericData == null)
                GenericData = Aurora.DataManager.DataManager.GetGenericPlugin();

            SceneObjectGroup groupclone = clone as SceneObjectGroup;
            SceneObjectGroup grouporiginal = clone as SceneObjectGroup;
            List<string> OriginalData = GenericData.Query("primUUID", grouporiginal.RootPart.UUID.ToString(), "auroraprims", "*");
            List<string> objectData = GenericData.Query("primUUID", groupclone.RootPart.UUID.ToString(), "auroraprims", "*");
            if (objectData.Count == 1)
            {
                if (OriginalData.Count != 1)
                {
                    OriginalData[0] = groupclone.RootPart.UUID.ToString();
                    GenericData.Insert("auroraprims", OriginalData.ToArray());
                }
                else
                {
                    CreateNewObjectData(groupclone.RootPart.UUID.ToString());
                    return;
                }
            }

            string Name = objectData[1];
            PrimTypes Type = (PrimTypes)Convert.ToInt32(objectData[2]);
            string AllKeys = objectData[3];
            string AllValues = objectData[4];
            string[] Keys = AllKeys.Split(',');
            string[] Values = AllValues.Split(',');
            if (Type != PrimTypes.Normal)
            {
                if (Type == PrimTypes.Bot)
                {
                    //Create bot, return object.
                    string FirstName = "";
                    string LastName = "";
                    string Appearance = "";
                    foreach (string Key in Keys)
                    {
                        if (Key.StartsWith("FirstName = "))
                        {
                            FirstName = Key.Split('=')[1];
                            FirstName.TrimStart(' ');
                        }
                        if (Key.StartsWith("LastName = "))
                        {
                            LastName = Key.Split('=')[1];
                            LastName.TrimStart('=');
                        }
                        if (Key.StartsWith("Appearance = "))
                        {
                            Appearance = Key.Split('=')[1];
                            Appearance.TrimStart(' ');
                        }
                    }
                    if (BotModule == null)
                        BotModule = m_scene.RequestModuleInterface<INPCModule>();
                    BotModule.CreateNPC(FirstName, LastName, groupclone.AbsolutePosition, groupclone.Scene, new UUID(Appearance));
                    SceneObjectGroup[] objs = new SceneObjectGroup[1];
                    objs[0] = groupclone;
                    groupclone.Scene.returnObjects(objs, groupclone.OwnerID);
                }
            }
        }

        void SceneContents_OnObjectRemove(EntityBase obj)
        {
        }

        void SceneContents_OnObjectCreate(EntityBase obj)
        {
            if (GenericData == null)
                GenericData = Aurora.DataManager.DataManager.GetGenericPlugin();

            SceneObjectGroup group = obj as SceneObjectGroup;
            List<string> objectData = GenericData.Query("primUUID", group.RootPart.UUID.ToString(), "auroraprims", "*");
            if (objectData.Count == 1)
            {
                CreateNewObjectData(group.RootPart.UUID.ToString());
                return;
            }

            string Name = objectData[1];
            PrimTypes Type = (PrimTypes)Convert.ToInt32(objectData[2]);
            string AllKeys = objectData[3];
            string AllValues = objectData[4];
            string[] Keys = AllKeys.Split(',');
            string[] Values = AllValues.Split(',');
            if (Type != PrimTypes.Normal)
            {
                if (Type == PrimTypes.Bot)
                {
                    //Create bot, return object.
                    string FirstName = "";
                    string LastName = "";
                    string Appearance = "";
                    foreach (string Key in Keys)
                    {
                        if (Key.StartsWith("FirstName = "))
                        {
                            FirstName = Key.Split('=')[1];
                            FirstName.TrimStart(' ');
                        }
                        if (Key.StartsWith("LastName = "))
                        {
                            LastName = Key.Split('=')[1];
                            LastName.TrimStart('=');
                        }
                        if (Key.StartsWith("Appearance = "))
                        {
                            Appearance = Key.Split('=')[1];
                            Appearance.TrimStart(' ');
                        }
                    }
                    if(BotModule == null)
                        BotModule = m_scene.RequestModuleInterface<INPCModule>();
                    BotModule.CreateNPC(FirstName, LastName, group.AbsolutePosition, group.Scene, new UUID(Appearance));
                    SceneObjectGroup[] objs = new SceneObjectGroup[1];
                    objs[0] = group;
                    group.Scene.returnObjects(objs, group.OwnerID);
                }
            }
        }

        public void PostInitialise()
        {
            BotModule = m_scene.RequestModuleInterface<INPCModule>();
        }

        public void Close(){}

        public string Name
        {
            get { return "AuroraPrimData"; }
        }

        public bool IsSharedModule
        {
            get { return true; }
        }   

        private void CreateNewObjectData(string itemID)
        {
            List<string> values = new List<string>();
            values.Add(itemID);
            values.Add("");
            values.Add("0");
            values.Add("");
            values.Add("");
            GenericData.Insert("auroraprims", values.ToArray());
        }
        
    }
}
