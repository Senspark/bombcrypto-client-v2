using System.Collections;
using System.Collections.Generic;
using Engine.Components;
using Engine.Entities;
using UnityEngine;

namespace Engine.Strategy.Shooter
{
    public interface IShooter
    {
        Entity Shoot(Shootable shootable);
    }
}