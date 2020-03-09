﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnimationControl
{

    public class CDClassInstance
    {
        public long UniqueID { get; }
        // Attributes
        public Dictionary<string, string> State { get; }

        public CDClassInstance(long UniqueID, List<CDAttribute> attributes)
        {
           this.State = new Dictionary<string, string>();

            foreach (CDAttribute Attribute in attributes)
            {
                this.State.Add(Attribute.Name, EXETypes.UnitializedName);
            }

            this.UniqueID = UniqueID;
            this.State.Add("unique_ID", UniqueID.ToString());
        }

        public String GetAttribute(String name)
        {
            String Result = null;
            if (this.State.ContainsKey(name))
            {
                Result = this.State[name];
            }

            return Result;
        }

        public int SetAttribute(String name, String value)
        {
            int success = -1;
            if (this.State.ContainsKey(name))
            {
                this.State[name] = value;
                success = 0;
            }

            return success;
        }
    }
}