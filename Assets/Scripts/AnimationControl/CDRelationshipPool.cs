using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace OALProgramControl
{
    public class CDRelationshipPool
    {
        private long RelationshipIdSeed;
        private List<CDRelationship> RelationshipPool { get; }

        public CDRelationshipPool()
        {
            this.RelationshipIdSeed = 1;
            this.RelationshipPool = new List<CDRelationship>();
        }

        public CDRelationship SpawnRelationship(String FromClass, String ToClass)
        {
            CDRelationship NewRelationship = new CDRelationship(FromClass, ToClass, "R" + this.RelationshipIdSeed);
            this.RelationshipPool.Add(NewRelationship);
            this.RelationshipIdSeed++;

            return NewRelationship;
        }
        public CDRelationship GetRelationship(String RelationshipName, String Class1, String Class2)
        {
            CDRelationship Result = null;
            foreach (CDRelationship Relationship in this.RelationshipPool)
            {
                if (Relationship.RelationshipName == RelationshipName && (Relationship.FromClass == Class1 && Relationship.ToClass == Class2) || (Relationship.FromClass == Class2 && Relationship.ToClass == Class1))
                {
                    Result = Relationship;
                    break;
                }
            }
            return Result;
        }
        public CDRelationship GetRelationshipByName(String RelationshipName)
        {
            CDRelationship Result = null;
            foreach (CDRelationship Relationship in this.RelationshipPool)
            {
                if (Relationship.RelationshipName == RelationshipName)
                {
                    Result = Relationship;
                    break;
                }
            }
            return Result;
        }
        public CDRelationship GetRelationshipByClasses(String Class1, String Class2)
        {
            CDRelationship Result = null;
            foreach (CDRelationship Relationship in this.RelationshipPool)
            {

                if ((Relationship.FromClass == Class1 && Relationship.ToClass == Class2) || (Relationship.FromClass == Class2 && Relationship.ToClass == Class1))
                {
                    Result = Relationship;
                    break;
                }
            }

            return Result;
        }
        public Boolean RelationshipExists(String RelationshipName, String Class1, String Class2)
        {
            Boolean Result = false;
            foreach (CDRelationship Relationship in this.RelationshipPool)
            {
                if (Relationship.RelationshipName == RelationshipName && (Relationship.FromClass == Class1 && Relationship.ToClass == Class2) || (Relationship.FromClass == Class2 && Relationship.ToClass == Class1))
                {
                    Result = true;
                    break;
                }
            }
            return Result;
        }
        public Boolean RelationshipExistsByName(String RelationshipName)
        {
            Boolean Result = false;
            foreach (CDRelationship Relationship in this.RelationshipPool)
            {
                if (Relationship.RelationshipName == RelationshipName)
                {
                    Result = true;
                    break;
                }
            }
            return Result;
        }
        public Boolean RelationshipExistsByClasses(String Class1, String Class2)
        {
            Boolean Result = false;
            foreach (CDRelationship Relationship in this.RelationshipPool)
            {
                if ((Relationship.FromClass == Class1 && Relationship.ToClass == Class2) || (Relationship.FromClass == Class2 && Relationship.ToClass == Class1))
                {
                    Result = true;
                    break;
                }
            }
            return Result;
        }
        public List<CDRelationship> GetAllRelationships() => RelationshipPool.ToList();

        public List<(String, long, long)> GetAllRelationshipsTupples()
        {
            List<(String, long, long)> Tupples = new List<(String, long, long)>();
            foreach (CDRelationship Relationship in this.RelationshipPool)
            {
                Tupples = Tupples.Concat(Relationship.GetTupples()).ToList();
            }
            return Tupples;
        }

        public void RemoveRelationship(CDRelationship relationship)
        {
            RelationshipPool.Remove(relationship);
        }
    }
}
