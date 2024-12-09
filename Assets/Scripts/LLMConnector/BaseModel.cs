using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

public abstract class BaseModel 
{
    public abstract Task<string> SendMessage(string message);
}