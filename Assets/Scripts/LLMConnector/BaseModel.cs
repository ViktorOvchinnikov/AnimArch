using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public abstract class GPTBase : MonoBehaviour
{
    public abstract Task<string> SendMessage(string message);
}