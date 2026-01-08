using System.Collections;
using System.Collections.Generic;
using Engine.Components;
using Engine.Entities;
using UnityEngine;

namespace Engine.Strategy.Spawner
{
    public interface ISpawner
    {
        Entity Spawn(Spawnable spawnable);
    }
}
