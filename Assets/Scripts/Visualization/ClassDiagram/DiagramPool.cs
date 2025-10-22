using AnimArch.Visualization.Diagrams;
using System.Collections.Generic;
using UnityEngine;
using Visualization.ClassDiagram.Diagrams;
using Visualization.ClassDiagram.Relations;

namespace Visualization.ClassDiagram
{
    public class DiagramPool : Singleton<DiagramPool>
    {
        public GameObject graphPrefab;
        public GameObject classPrefab;
        public GameObject objectPrefab;
        public GameObject activityPrefab;
        public GameObject activityInitialPrefab;
        public GameObject activityFinalPrefab;
        public GameObject classAttributePrefab;
        public GameObject classMethodPrefab;
        public GameObject parameterMethodPrefab;
        public GameObject associationNonePrefab;
        public GameObject associationFullPrefab;
        public GameObject associationSDPrefab;
        public GameObject associationDSPrefab;
        public GameObject dependsPrefab;
        public GameObject generalizationPrefab;
        public GameObject implementsPrefab;
        public GameObject realisationPrefab;
        public GameObject interGraphLinePrefab;
        public GameObject interGraphArrowPrefab;

        public GameObject networkGraphPrefab;
        public GameObject networkClassPrefab;
        public GameObject networkUnitsPrefab;

        public GameObject networkAssociationDSPrefab;
        public GameObject networkAssociationFullPrefab;
        public GameObject networkAssociationNonePrefab;
        public GameObject networkAssociationSDPrefab;
        public GameObject networkGeneralizationPrefab;
        public GameObject networkDependsPrefab;
        public GameObject networkRealisationPrefab;

        public Diagrams.ClassDiagram ClassDiagram;
        public ObjectDiagram ObjectDiagram;
        public ActivityDiagram ActivityDiagram;

        public List<InterGraphRelation> RelationsClassToObject = new();
        
        // DiffResult for storing diagram differences
        public DiffResult CurrentDiffResult { get; set; }
    }
}
