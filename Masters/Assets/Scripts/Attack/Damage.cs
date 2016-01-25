﻿using UnityEngine;

namespace Assets.Scripts.Attack
{
    public class Damage : MonoBehaviour
    {
        public Avatar caster;
        public AudioSource punchSound;
        public float damage;

        public void OnTriggerEnter2D(Collider2D other)
        {
            var avatar = other.gameObject.GetComponent<Avatar>();
            if (avatar && avatar != caster)
            {
                avatar.damage(damage, caster);
                if(punchSound!=null)punchSound.Play();
            }
        }
    }
}