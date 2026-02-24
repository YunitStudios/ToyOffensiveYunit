using UnityEngine;
using System.Collections.Generic;

namespace Utility
{
    public class RaycastCone
    {
        private Vector3 Origin { get; set; }
        private Vector3 Direction { get; set; }
        private float Range { get; set; }
        private float Angle { get; set; }
        private LayerMask LayerMask { get; set; }
        private Vector3 Right => Quaternion.AngleAxis(90, Vector3.up) * Direction;


        private readonly Vector3 _forward;
        private readonly List<Ray> _rays = new List<Ray>();

        /// <summary>
        /// creates a new RaycastCone. angle is the angle between two opposite edges which defaults to 90 degrees
        /// </summary>
        public RaycastCone(Vector3 origin, Vector3 direction, float range, LayerMask layerMask, float angle = 90)
        {
            Origin = origin;
            Direction = direction;
            Range = range;
            Angle = angle;
            LayerMask = layerMask;
            _forward = direction.normalized;
        }
        
        /// <summary>
        /// casts the cone of rays, returning true if any rays hit.
        /// hits is a list of all the RaycastHit objects
        /// </summary>
        public static bool Raycast(RaycastCone cone, out IList<RaycastHit> hits)
        {
            // constructing the rays
            cone._rays.Add(new Ray(cone.Origin, cone.Direction));
            cone._rays.Add(new Ray(cone.Origin, Quaternion.AngleAxis(-cone.Angle / 2, Vector3.up) * cone.Direction));
            cone._rays.Add(new Ray(cone.Origin, Quaternion.AngleAxis(-cone.Angle / 4, Vector3.up) * cone.Direction));
            cone._rays.Add(new Ray(cone.Origin, Quaternion.AngleAxis(cone.Angle / 2, Vector3.up) * cone.Direction));
            cone._rays.Add(new Ray(cone.Origin, Quaternion.AngleAxis(cone.Angle / 4, Vector3.up) * cone.Direction));
            cone._rays.Add(new Ray(cone.Origin, Quaternion.AngleAxis(-cone.Angle / 2, cone.Right) * cone.Direction));
            cone._rays.Add(new Ray(cone.Origin, Quaternion.AngleAxis(-cone.Angle / 4, cone.Right) * cone.Direction));
            cone._rays.Add(new Ray(cone.Origin, Quaternion.AngleAxis(cone.Angle / 2, cone.Right) * cone.Direction));
            cone._rays.Add(new Ray(cone.Origin, Quaternion.AngleAxis(cone.Angle / 4, cone.Right) * cone.Direction));
            
            // casting rays and recording any that hit
            hits = new List<RaycastHit>();
            foreach (Ray ray in cone._rays)
            {
                bool valid = Physics.Raycast(ray, out RaycastHit hit, cone.Range, cone.LayerMask);
                if (valid)
                    hits.Add(hit);
                Debug.DrawRay(ray.origin, ray.direction * cone.Range, valid ? Color.cyan : Color.magenta);
            }
            return hits.Count > 0;
        }
        
        /// <summary>
        /// casts the cone of rays, returning true if any rays hit.
        /// calculates the average surface normal of the rays that hit 
        /// </summary>
        public bool CastRays(out IList<RaycastHit> hits, out Vector3 surfaceNormal)
        {
            bool result = Raycast(this, out hits);

            // averaging surface normals of rays that hit
            surfaceNormal = Vector3.zero;
            foreach (RaycastHit hit in hits)
            {
                surfaceNormal += hit.normal;
            }
            surfaceNormal.Normalize();
            return result;
        }

        /// <summary>
        /// casts the cone of rays, returning true if any rays hit.
        /// calculates the average surface normal of the rays that hit 
        /// </summary>
        public bool CastRays(out Vector3 surfaceNormal) => CastRays(out IList<RaycastHit> _, out surfaceNormal);

        /// <summary>
        /// casts the cone of rays, returning true if any rays hit.
        /// </summary>
        public bool CastRays() => Raycast(this, out IList<RaycastHit> _);
        
        /// <summary>
        /// casts the cone of rays, returning true if any rays hit.
        /// hits is a list of all the RaycastHit objects
        /// </summary>
        public bool CastRays(out IList<RaycastHit> hits) => Raycast(this, out hits);


        /// <summary>
        /// rotates the ray to the right such that the leftmost ray now points forward 
        /// </summary>
        public void LookRight()
        {
            Direction = Quaternion.AngleAxis(Angle / 2, Vector3.up) * _forward;
        }
        
        /// <summary>
        /// rotates the ray to the left such that the rightmost ray now points forward 
        /// </summary>
        public void LookLeft()
        {
            Direction = Quaternion.AngleAxis(-Angle / 2, Vector3.up) * _forward;
        }

        /// <summary>
        /// rotates the ray upwards such that the bottom ray now points forward 
        /// </summary>
        public void LookUp()
        {
            Direction = Quaternion.AngleAxis(-Angle / 2, Right) * _forward;
        }

        /// <summary>
        /// rotates the ray downwards such that the top ray now points forward 
        /// </summary>
        public void LookDown()
        {
            Direction = Quaternion.AngleAxis(Angle / 2, Right) * _forward;
        }
    }
}