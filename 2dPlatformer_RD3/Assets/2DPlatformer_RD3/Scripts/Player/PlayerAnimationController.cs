using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Player
{
    public class PlayerAnimationController : MonoBehaviour
    {
        private Animator animator;

        // Animation states
        public readonly string PLAYER_IDLE = "player_idle";
        public readonly string PLAYER_RUN = "player_run";
        public readonly string PLAYER_JUMP = "player_jump";
        public readonly string PLAYER_WALL_GRAB = "player_wallGrab";
        public readonly string PLAYER_WALL_CLIMB = "player_wallClimb";
        private string currentState;

        void Start()
        {
            animator = GetComponent<Animator>();
        }

        public void ChangeAnimationState(string newState)
        {
            if (currentState == newState) return; // prevent animation from interupting itself
            animator.Play(newState);
            currentState = newState;
        }
    }
}