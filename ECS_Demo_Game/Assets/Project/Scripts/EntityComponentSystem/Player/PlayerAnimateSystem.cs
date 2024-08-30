//using System.Collections;
//using System.Collections.Generic;
//using Unity.Entities;
//using UnityEngine;

//public partial struct PlayerAnimateSystem : ISystem
//{
//    public void OnUpdate(ref SystemState state)
//    {
//        foreach (var (playerGameObjectPrefab, entity) in SystemAPI.Query<PlayerGameObjectPrefab>().WithNone<PlayerAnimationReference>().WithEntityAccess())
//        {
//            var instance = Object.Instantiate(playerGameObjectPrefab.value);

//            var animatorReference = new PlayerAnimationReference
//            {
//                value = instance.GetComponent<Animator>(),
//            };

//            state.Enabled = false;
//        }
//    }
//}
