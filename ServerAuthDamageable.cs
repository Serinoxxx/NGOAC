using MalbersAnimations.Reactions;
using Mono.CSharp;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace MalbersAnimations.NetCode
{
    public class ServerAuthDamageable : MDamageable
    {
        [SerializeField] NetworkDamageable networkDamageable;

        public override void ReceiveDamage(Vector3 Direction, Vector3 Position, GameObject Damager, StatModifier damage, bool isCritical, bool react, Reaction customReaction, bool pureDamage, StatElement element)
        {
            //Prevent clients from receiving damage unless it originated from the server
            if (!networkDamageable.IsServer && Damager != gameObject)
            {
                return;
            }
            base.ReceiveDamage(Direction, Position, Damager, damage, isCritical, react, customReaction, pureDamage, element);
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(ServerAuthDamageable))]
    public class ServerAuthDamageableEditor : MDamageableEditor
    {
    }
#endif
}
