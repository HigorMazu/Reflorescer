using Godot;
using System;
using Joguim.Core;

namespace Joguim.Companion
{
    public partial class FaiscaController : Node2D
    {
        [Export] public float FollowDistance = 40.0f;
        [Export] public float FollowHeight = -30.0f;
        [Export] public float FollowSpeed = 3.0f;
        [Export] public float HoverAmplitude = 5.0f;
        [Export] public float HoverSpeed = 2.0f;
        [Export] public float WallAvoidanceDistance = 15.0f;
        [Export] public NodePath SpritePath;
        [Export] public NodePath LightPath;

        public AnimatedSprite2D Sprite { get; private set; }
        public PointLight2D Light { get; private set; }

        public FaiscaStateMachine StateMachine { get; private set; }
        public bool InvestigationComplete { get; set; }

        private Node2D _player;
        private float _hoverTimer;
        private Vector2 _currentVelocity;
        private Vector2 _targetPosition;

        public override void _Ready()
        {
            Sprite = GetNodeOrNull<AnimatedSprite2D>(SpritePath);
            Light = GetNodeOrNull<PointLight2D>(LightPath);

            StateMachine = GetNodeOrNull<FaiscaStateMachine>("FaiscaStateMachine");
            if (StateMachine == null)
            {
                GD.PrintErr("FaiscaController: FaiscaStateMachine node not found.");
            }
            else
            {
                StateMachine.Initialize(this);
                StateMachine.ChangeState(FaiscaStateType.Follow);
            }

            _player = GetTree().GetFirstNodeInGroup("Player") as Node2D;

            EventBus.Instance.PlayerRespawned += OnPlayerRespawned;
        }

        // O EventBus é autoload e sobrevive à troca de cena: sem desinscrever, o handler de uma Faísca
        // já destruída lança ObjectDisposedException e interrompe os demais inscritos do evento.
        public override void _ExitTree()
        {
            if (EventBus.Instance != null) EventBus.Instance.PlayerRespawned -= OnPlayerRespawned;
        }

        public override void _PhysicsProcess(double delta)
        {
            if (_player == null)
            {
                _player = GetTree().GetFirstNodeInGroup("Player") as Node2D;
            }
        }

        public void FollowPlayer(double delta)
        {
            if (_player == null) return;

            Vector2 targetPos = GetTargetPosition();

            Vector2 spaceState = targetPos - GlobalPosition;
            float distance = spaceState.Length();

            _currentVelocity = _currentVelocity.Lerp(
                spaceState * FollowSpeed,
                (float)delta * 8f
            );

            GlobalPosition += _currentVelocity * (float)delta;

            if (Light != null)
            {
                Light.TextureScale = 1.0f + Mathf.Sin(_hoverTimer * HoverSpeed * 2f) * 0.1f;
            }

            UpdateFacing();
        }

        public void Hover(double delta)
        {
            _hoverTimer += (float)delta;
            float hoverOffset = Mathf.Sin(_hoverTimer * HoverSpeed) * HoverAmplitude;
            Position = new Vector2(Position.X, FollowHeight + hoverOffset);
        }

        public Vector2 GetTargetPosition()
        {
            if (_player == null) return GlobalPosition;

            var playerController = _player as Player.PlayerController;
            float facingX = playerController != null ? playerController.LastFacingDirection.X : 1f;

            Vector2 targetPos = _player.GlobalPosition + new Vector2(
                -facingX * FollowDistance,
                FollowHeight
            );

            var spaceState = GetWorld2D().DirectSpaceState;
            var query = new PhysicsRayQueryParameters2D
            {
                From = _player.GlobalPosition,
                To = targetPos,
                CollisionMask = 1
            };
            var result = spaceState.IntersectRay(query);

            if (result.Count > 0)
            {
                var collisionPoint = (Vector2)result["position"];
                targetPos = _player.GlobalPosition + (collisionPoint - _player.GlobalPosition) * 0.5f;
            }

            return targetPos;
        }

        private void UpdateFacing()
        {
            if (_player == null || Sprite == null) return;

            bool isLeft = _player.GlobalPosition.X > GlobalPosition.X;
            Sprite.FlipH = isLeft;
        }

        public void PlayAnimation(string animName)
        {
            if (Sprite != null && Sprite.SpriteFrames != null && Sprite.SpriteFrames.HasAnimation(animName))
            {
                Sprite.Play(animName);
            }
        }

        public void SetLightEnabled(bool enabled)
        {
            if (Light != null)
            {
                Light.Visible = enabled;
            }
        }

        public void SetLightRadius(float radius)
        {
            if (Light != null)
            {
                Light.TextureScale = radius / 64f;
            }
        }

        public void SetLightColor(Color color)
        {
            if (Light != null)
            {
                Light.Color = color;
            }
        }

        private void OnPlayerRespawned(Vector2 position)
        {
            GlobalPosition = position + new Vector2(FollowDistance, FollowHeight);
            _currentVelocity = Vector2.Zero;

            if (StateMachine != null)
            {
                StateMachine.ChangeState(FaiscaStateType.Follow);
            }
        }
    }
}
