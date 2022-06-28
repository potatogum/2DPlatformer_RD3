using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
/// This is a pretty filthy script. I was just arbitrarily adding to it as I went.
/// You won't find any programming prowess here.
/// This is a supplementary script to help with effects and animation. Basically a juice factory.
/// </summary>
namespace TarodevController {
    public class PlayerAnimator : MonoBehaviour {
        [SerializeField] private Animator _anim;
        // bones' serialized variables
        [SerializeField] private bool _facingRight = true;

        private IPlayerController _player;


        void Awake() {
            _player = GetComponentInParent<IPlayerController>();

            // add to event listeners
            _player.OnGroundedChanged += OnLanded;
            _player.OnJumping += OnJumping;
            _player.OnDoubleJumping += OnDoubleJumping;
            _player.OnDashingChanged += OnDashing;
            _player.OnCrouchingChanged += OnCrouching;
        }


        void OnDestroy() {
            _player.OnGroundedChanged -= OnLanded;
            _player.OnJumping -= OnJumping;
            _player.OnDoubleJumping -= OnDoubleJumping;
            _player.OnDashingChanged -= OnDashing;
            _player.OnCrouchingChanged -= OnCrouching;
        }

        private void OnDoubleJumping() {

        }

        private void OnDashing(bool dashing) {

        }


        private void OnJumping() {

        }


        private void OnLanded(bool grounded) {

        }

        private void OnCrouching(bool crouching) {

        }

        void FixedUpdate() {
            if (_player == null) return;

            var inputPoint = Mathf.Abs(_player.Input.X);

            // Flip the sprite
            //if (_player.Input.X != 0) transform.localScale = new Vector3(_player.Input.X > 0 ? 1 : -1, 1, 1);
            if (_player.Input.X < 0 && _facingRight) {
                FlipSprite();
            } 
            else if (_player.Input.X > 0 && !_facingRight) {
                FlipSprite();
            }
            if (_player.Input.X != 0 && _player.Grounded) {
                _anim.SetBool("Running", true);
            } else {
                _anim.SetBool("Running", false);
            }

        
        }

        void FlipSprite() {
            Vector3 currentScale = gameObject.transform.localScale;
            currentScale.x *= -1;
            gameObject.transform.localScale = currentScale;
            _facingRight = !_facingRight;
        }
        
    }

}