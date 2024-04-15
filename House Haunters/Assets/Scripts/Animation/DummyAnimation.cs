using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// for moves that have not had animations added yet
public class DummyAnimation : IMoveAnimator
{
    public bool Completed => true;

    public void Start()
    {
            
    }

    public void Update(float deltaTime)
    {
    }
}
