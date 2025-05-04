using System.Collections.Generic;
using Assets.Scripts.AnimationControl.UMLDiagram;
using OALProgramControl;
using UnityEngine.UI;
using UnityEngine;
using TMPro;
using Visualization.Animation;
using Visualization.ClassDiagram;
using System.Threading.Tasks;
using UnityEngine.UI.Extensions;
using Visualization.ClassDiagram.Diagrams;
using Visualization.ClassDiagram.Editors;
using Visualization.ClassDiagram.MarkedDiagram;
using Visualization.ClassDiagram.Relations;
using Visualization.UI;


public class SuggestedDiagram : MonoBehaviour
{
    public static async void Test()
    {
        await Task.Delay(5000);
        // PlantUMLBuilder plantUmlBuilder = new PlantUMLBuilder();
        // string plantUMLString = plantUmlBuilder.GetDiagram();
        string plantUMLString = @"
@startuml
class AbstractFactory {
    + CreateWarrior() : Warrior
    + CreateRanger() : Ranger
    + CreateMage() : Mage
}
class Client {
    + RunGame() : void
}
class ElvenFactory {
    + CreateWarrior() : Warrior
    + CreateRanger() : Ranger
    + CreateMage() : Mage
    + CreateWarrior() : Warrior
    + CreateRanger() : Ranger
    + CreateMage() : Mage
}
class ElvenMage {
    + Attack() : void
    + ChooseSpell() : void
    + CastSpell() : void
    + ElvenMage() : ElvenMage
    + Attack() : void
    + ChooseSpell() : void
    + CastSpell() : void
}
class ElvenRanger {
    + Attack() : void
    + Hide() : void
    + SneakAttack() : void
    + ElvenRanger() : ElvenRanger
    + Attack() : void
    + Hide() : void
    + SneakAttack() : void
}
class Game {
    + CreateLevel() : void
    + CreateElvenArmy() : void
    + CreateTrollArmy() : void
    + CreateHumanArmy() : void
}
class HumanFactory {
    + CreateWarrior() : Warrior
    + CreateRanger() : Ranger
    + CreateMage() : Mage
    + CreateWarrior() : Warrior
    + CreateRanger() : Ranger
    + CreateMage() : Mage
}
class HumanMage {
    + Attack() : void
    + ChooseSpell() : void
    + CastSpell() : void
    + HumanMage() : HumanMage
    + Attack() : void
    + ChooseSpell() : void
    + CastSpell() : void
    + DoIt1() : void
    + DoIt2() : void
}
class HumanRanger {
    + Attack() : void
    + SneakAttack() : void
    + HumanRanger() : HumanRanger
    + Attack() : void
    + SneakAttack() : void
}
class HumanWarrior {
    + Provoke() : void
    + Block() : void
    + Attack() : void
    + HumanWarrior() : HumanWarrior
    + Provoke() : void
    + Block() : void
    + Attack() : void
}
class Mage {
    + Attack() : void
    + ChooseSpell() : void
    + CastSpell() : void
}
class Ranger {
    + Attack() : void
    + Hide() : void
    + SneakAttack() : void
}
class TrollFactory {
    + CreateWarrior() : Warrior
    + CreateRanger() : Ranger
    + CreateMage() : Mage
    + CreateWarrior() : Warrior
    + CreateRanger() : Ranger
    + CreateMage() : Mage
}
class TrollMage {
    + Attack() : void
    + ChooseSpell() : void
    + CastSpell() : void
    + TrollMage() : TrollMage
    + Attack() : void
    + ChooseSpell() : void
    + CastSpell() : void
}
class TrollRanger {
    + Attack() : void
    + Hide() : void
    + SneakAttack() : void
    + TrollRanger() : TrollRanger
    + Attack() : void
    + Hide() : void
    + SneakAttack() : void
}
class TrollWarrior {
    + Provoke() : void
    + Block() : void
    + Attack() : void
    + TrollWarrior() : TrollWarrior
    + Provoke() : void
    + Block() : void
    + Attack() : void
}
class Warrior {
    + Provoke() : void
    + Block() : void
    + Attack() : void
}
class SuperWarrior {
    + Provoke() : void
    + Block() : void
    + Attack() : void
}
HumanFactory --|> AbstractFactory
TrollFactory --|> AbstractFactory
ElvenFactory --|> AbstractFactory
Game --> AbstractFactory
Client --> Game
ElvenFactory --> ElvenRanger
ElvenFactory --> ElvenMage
Game --> ElvenFactory
ElvenMage --|> Mage
ElvenRanger --|> Ranger
Game --> HumanFactory
Game --> Mage
Game --> Warrior
Game --> TrollFactory
Game --> Ranger
HumanFactory --> HumanMage
HumanFactory --> HumanRanger
HumanFactory --> HumanWarrior
HumanMage --|> Mage
HumanRanger --|> Ranger
HumanWarrior --|> Warrior
TrollMage --|> Mage
TrollRanger --|> Ranger
TrollFactory --> TrollWarrior
TrollFactory --> TrollRanger
TrollFactory --> TrollMage
HumanFactory --> Game
@enduml";
        Debug.Log(plantUMLString);
        ClassDiagramManager manager = UMLParserBridge.Parse(plantUMLString);
        
        ClassDiagramDiffer differ = ClassDiagramDiffer.CreateClassDiagramDifferWithCurrentDiagram();
        DiffResult diff = differ.GetDifference(manager);
        
        ClassDiagramChangesVisualizer visualizer = new ClassDiagramChangesVisualizer(diff);
        visualizer.Visualize();

        DiagramPool.Instance.ClassDiagram.graph.Layout();

        // GPTMessage gptMessage = new GPTMessage();
        // string systemPrompt = @"
        //                       Imagine you're an experienced software engineer. You will be given a UML diagram in the form of PlantUML code. Your task is to suggest a couple of small changes that the user would most likely want to make in the next step of their work. The changes don't have to be significant. Your limit on the number of changes: 3. Changes can be such as: 
        //                       - Adding/Removing relations/classes/methods or class attributes
        //                       - Changing the name of a class/method/attribute
        //                       Your answer should contain only PlantUML code. (starting with the @startuml tag and ending with @enduml). The PlantUML code is provided below.;
        //                       ";
        //
        // string fullPrompt = systemPrompt + "\n" + plantUMLString;
        //
        // GPTMessage message = new GPTMessage();
        //
        // string response = await message.SendMessage("Hello how are you?");
        //
        // Debug.Log($"Response GPT: {response}");

        // SetClassColorAndButtons("suggested_class", new Color(0f, 1f, 0f, 0.5f));
        // SetClassColorAndButtons("HumanWarrior", new Color(1f, 0f, 0f, 0.5f));
        // ActivateMethods("HumanRanger");
    }
}
