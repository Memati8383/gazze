using UnityEngine;
using UnityEngine.UI;

namespace Gazze.UI
{
    /// <summary>
    /// Başarım bildirimine ek görsel efektler ekler
    /// </summary>
    [RequireComponent(typeof(AchievementNotification))]
    public class AchievementNotificationEffects : MonoBehaviour
    {
        [Header("Particle Effects")]
        [SerializeField] private ParticleSystem sparkleEffect;
        [SerializeField] private bool autoCreateParticles = true;
        
        [Header("Glow Pulse")]
        [SerializeField] private Image glowImage;
        [SerializeField] private float pulseSpeed = 2f;
        [SerializeField] private float pulseIntensity = 0.3f;
        
        [Header("Icon Animation")]
        [SerializeField] private Image iconImage;
        [SerializeField] private float iconBounceAmount = 0.15f;
        [SerializeField] private float iconBounceSpeed = 3f;
        
        private float animationTime = 0f;
        private Vector3 iconOriginalScale;
        private Color glowOriginalColor;
        
        private void Awake()
        {
            if (iconImage != null)
            {
                iconOriginalScale = iconImage.transform.localScale;
            }
            
            if (glowImage != null)
            {
                glowOriginalColor = glowImage.color;
            }
            
            if (autoCreateParticles && sparkleEffect == null)
            {
                CreateSparkleEffect();
            }
        }
        
        private void OnEnable()
        {
            animationTime = 0f;
            
            if (sparkleEffect != null)
            {
                sparkleEffect.Play();
            }
        }
        
        private void Update()
        {
            animationTime += Time.deltaTime;
            
            // Glow pulse efekti
            if (glowImage != null)
            {
                float pulse = Mathf.Sin(animationTime * pulseSpeed) * pulseIntensity;
                Color targetColor = glowOriginalColor;
                targetColor.a = glowOriginalColor.a + pulse;
                glowImage.color = targetColor;
            }
            
            // Icon bounce efekti
            if (iconImage != null)
            {
                float bounce = Mathf.Sin(animationTime * iconBounceSpeed) * iconBounceAmount;
                iconImage.transform.localScale = iconOriginalScale * (1f + bounce);
            }
        }
        
        private void CreateSparkleEffect()
        {
            GameObject particleObj = new GameObject("SparkleEffect");
            particleObj.transform.SetParent(transform, false);
            
            sparkleEffect = particleObj.AddComponent<ParticleSystem>();
            var main = sparkleEffect.main;
            main.startLifetime = 1f;
            main.startSpeed = 2f;
            main.startSize = 0.1f;
            main.startColor = new Color(1f, 0.84f, 0f, 1f);
            main.maxParticles = 20;
            main.loop = false;
            main.playOnAwake = false;
            
            var emission = sparkleEffect.emission;
            emission.rateOverTime = 0;
            emission.SetBursts(new ParticleSystem.Burst[]
            {
                new ParticleSystem.Burst(0f, 25), // Daha fazla parçacık
                new ParticleSystem.Burst(0.2f, 15)
            });
            
            var shape = sparkleEffect.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 1f;
            
            var colorOverLifetime = sparkleEffect.colorOverLifetime;
            colorOverLifetime.enabled = true;
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[] 
                { 
                    new GradientColorKey(Color.white, 0f),
                    new GradientColorKey(new Color(1f, 0.84f, 0f), 1f)
                },
                new GradientAlphaKey[] 
                { 
                    new GradientAlphaKey(1f, 0f),
                    new GradientAlphaKey(0f, 1f)
                }
            );
            colorOverLifetime.color = new ParticleSystem.MinMaxGradient(gradient);
            
            var sizeOverLifetime = sparkleEffect.sizeOverLifetime;
            sizeOverLifetime.enabled = true;
            AnimationCurve sizeCurve = new AnimationCurve();
            sizeCurve.AddKey(0f, 1f);
            sizeCurve.AddKey(1f, 0f);
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);
            
            var renderer = sparkleEffect.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.material = new Material(Shader.Find("Sprites/Default"));
        }
    }
}
