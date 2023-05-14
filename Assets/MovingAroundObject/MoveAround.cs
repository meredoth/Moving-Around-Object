using System;
using System.Collections.Generic;
using UnityEngine;

namespace MovingAroundObject
{
public class MoveAround : MonoBehaviour
{
   private enum MovementType
   {
      Towards,
      Away,
      Attack
   }
   
   [SerializeField] private SpriteRenderer rd;

   [Header("Parameters")] 
   [SerializeField] private Transform center;
   [SerializeField] private float maxRadius;
   [SerializeField] private float minRadius;
   [SerializeField] private float speed;
   [SerializeField] private float degrees;
   [SerializeField] private float delayBetweenPoints;
   [SerializeField] private bool clockwise;

   private Vector2 _newPosition;
   private float _delay;
   private Transform _thisTransform;
   private Color _color;
   private bool _attackPositionCalculated;
   private MovementType _movementType;
   private readonly Dictionary<MovementType, Action> _perform = new();
   private void Start()
   {
      _thisTransform = transform;
      _perform.Add(MovementType.Towards,() => Hunt(center.position, true));
      _perform.Add(MovementType.Away, () => Hunt(center.position, false));
      _perform.Add(MovementType.Attack, Attack);
   }

   private void Update()
   {
      _thisTransform.right = _thisTransform.position - center.position;
      _perform[_movementType]();
   }

   private void Attack()
   {
      if (!_attackPositionCalculated)
         _newPosition = AttackPosition();

      _thisTransform.position = MoveTo(_newPosition, speed * Time.deltaTime);

      if (_thisTransform.position.Equals(_newPosition))
      {
         Fire();
         _movementType = GetMovementType(_newPosition);
      }

      #region Local Methods
      Vector2 AttackPosition()
      {
         _attackPositionCalculated = true;
         _delay = 0f;
         return CalculatePosition(center, degrees, clockwise);
      }
      
      Vector2 CalculatePosition(Transform target, float angle, bool clockWise = false)
      {
         var targetPos = (Vector2)target.position;
         var position = (Vector2)transform.position;
         var angleInRadians = clockWise ? -angle * Mathf.Deg2Rad : angle * Mathf.Deg2Rad;

         var cosOfAngle = Mathf.Cos(angleInRadians);
         var sinOfAngle = Mathf.Sin(angleInRadians);

         var initialVector = new Vector2(cosOfAngle, sinOfAngle);
         var perpendicularVectorToInitial = new Vector2(-sinOfAngle, cosOfAngle);

         var newPositionX = (position.x - targetPos.x) * initialVector;
         var newPositionY = (position.y - targetPos.y) * perpendicularVectorToInitial;
         var finalPosition = newPositionX + newPositionY + targetPos;

         return finalPosition;
      }
      
      void Fire()
      {
         if (rd.color == Color.white)
            rd.color = Color.red;

         _delay += Time.deltaTime;
      
         if (_delay >= delayBetweenPoints)
         {
            _newPosition = AttackPosition();
            rd.color = Color.white;
         }
      }
      #endregion
   }

   private void Hunt(Vector2 pos, bool towards)
   {
      if (rd.color != Color.white) 
         rd.color = Color.white;
      
      _attackPositionCalculated = false;
      
      _thisTransform.position = towards ? 
         MoveTo(pos, speed * Time.deltaTime) : MoveTo(pos, -speed * Time.deltaTime);
      
      _movementType = GetMovementType(_thisTransform.position);
   }
   
   private MovementType GetMovementType(Vector3 pos)
   {
      if ( (pos - center.position).magnitude > maxRadius)
         return MovementType.Towards;

      if ((pos - center.position).magnitude < minRadius)
         return MovementType.Away;

      return MovementType.Attack;
   }

   private Vector2 MoveTo(Vector2 pos, float velocity) => 
      Vector2.MoveTowards(transform.position, pos, velocity);
}
}