using Fusion;
using FusionHelpers;
using UnityEngine;
using Random = UnityEngine.Random;

namespace FusionExamples.Tanknarok
{
	public class PowerupSpawner : NetworkBehaviourWithState<PowerupSpawner.NetworkState> 
	{
		[Networked] public override ref NetworkState State => ref MakeRef<NetworkState>();
		public struct NetworkState : INetworkStruct
		{
			public TickTimer respawnTimer;
			public int activePowerupIndex;
		}

		[SerializeField] private Renderer _renderer;
		[SerializeField] private MeshFilter _meshFilter;
		[SerializeField] private MeshRenderer _rechargeCircle;

		[Header("Colors")] 
		[SerializeField] private Color _mainPowerupColor;
		[SerializeField] private Color _specialPowerupColor;
		[SerializeField] private Color _buffPowerupColor;

		const float RESPAWN_TIME = 3f;

		private static readonly int Recharge = Shader.PropertyToID("_Recharge");
		private AudioEmitter _audio;

		public override void Spawned()
		{
			_audio = GetComponent<AudioEmitter>();
			if(Object.HasStateAuthority)
				SetNextPowerup();
			InitPowerupVisuals();
		}

		public override void Render()
		{
			if ( TryGetStateChanges(out var old, out var current) )
			{
				if(old.respawnTimer.TargetTick>0) // Avoid triggering sound effect on initial init
					_audio.PlayOneShot(GetPowerup(old.activePowerupIndex).pickupSnd);
				InitPowerupVisuals();
			}

			float progress = 0;
			if (!State.respawnTimer.Expired(Runner))
				progress = 1.0f - (State.respawnTimer.RemainingTime(Runner) ?? 0) / RESPAWN_TIME;
			else
				_renderer.transform.localScale = Vector3.Lerp(_renderer.transform.localScale, Vector3.one, Time.deltaTime * 5f);
			_rechargeCircle.material.SetFloat(Recharge, progress);
		}

		private void OnTriggerStay(Collider collisionInfo)
		{
			if (!State.respawnTimer.Expired(Runner))
				return;

			Player player = collisionInfo.gameObject.GetComponent<Player>();
			if (!player)
				return;

			int powerup = State.activePowerupIndex;
			player.RaiseEvent( new Player.PickupEvent { powerup = powerup} );

			SetNextPowerup();
		}
		
		private void InitPowerupVisuals()
		{
			var powerup = GetPowerup(State.activePowerupIndex);

			// 1) Asegurar referencias al renderer visual correcto
			//    (busca en el mismo objeto del MeshFilter primero, luego en hijos/padres)
			var visualRenderer = _meshFilter != null 
				? _meshFilter.GetComponent<MeshRenderer>() 
				: null;
			if (visualRenderer == null)
				visualRenderer = GetComponentInChildren<MeshRenderer>(true);
			if (visualRenderer == null)
				visualRenderer = GetComponent<MeshRenderer>();

			// (opcional) si fuera Skinned:
			var skinned = (visualRenderer == null) ? GetComponentInChildren<SkinnedMeshRenderer>(true) : null;

			_renderer = (Renderer)(object)visualRenderer ?? skinned; // usa el que exista

			// 2) Asignar la malla seleccionada en el ScriptableObject
			_meshFilter.sharedMesh = powerup.powerupSpawnerMesh;

			// 3) Eliminar posibles overrides de MaterialPropertyBlock
			if (_renderer != null)
				_renderer.SetPropertyBlock(null);

			// 4) Asignar material del powerup a TODOS los submeshes
			if (_renderer != null && powerup.powerupMaterial != null)
			{
				int subCount = _meshFilter.sharedMesh != null ? _meshFilter.sharedMesh.subMeshCount : 1;

				var mats = _renderer.sharedMaterials;
				if (mats == null || mats.Length != subCount)
					System.Array.Resize(ref mats, subCount);

				for (int i = 0; i < subCount; i++)
					mats[i] = powerup.powerupMaterial;

				_renderer.sharedMaterials = mats;
			}

			// 5) Efecto de aparición (escala)
			_renderer.transform.localScale = Vector3.zero;

			// 6) El aro de recarga sigue con su propio material/color
			if (_rechargeCircle != null)
				_rechargeCircle.material.color = GetPowerupColor(powerup.weaponInstallationType);
		}



		private void SetNextPowerup()
		{
			State.respawnTimer = TickTimer.CreateFromSeconds(Runner, RESPAWN_TIME);
			GetPowerup(0); // Force load of powerups
			// Strictly speaking, this isn't correct since it will assign different values on proxies.
			// However, we rely on this being updated from StateAuth before the respawnTimer expires. 
			State.activePowerupIndex = Random.Range(0, _powerupElements.Length);
		}

		private Color GetPowerupColor(WeaponManager.WeaponInstallationType weaponType)
		{
			switch (weaponType)
			{
				default:
				case WeaponManager.WeaponInstallationType.PRIMARY: return _mainPowerupColor;
				case WeaponManager.WeaponInstallationType.SECONDARY: return _specialPowerupColor;
				case WeaponManager.WeaponInstallationType.BUFF: return _buffPowerupColor;
			}
		}
		
		private static PowerupElement[] _powerupElements;
		public static PowerupElement GetPowerup(int powerupIndex)
		{
			if (_powerupElements == null)
			{
				_powerupElements = Resources.LoadAll<PowerupElement>("PowerupElements");
			}
			return _powerupElements[powerupIndex];
		} 
	}
}