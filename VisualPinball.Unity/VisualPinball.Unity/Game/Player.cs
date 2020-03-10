﻿using System.Collections.Generic;
using System.Linq;
using Unity.Entities;
using UnityEngine;
using VisualPinball.Engine.Game;
using VisualPinball.Engine.VPT.Flipper;
using VisualPinball.Engine.VPT.Kicker;
using VisualPinball.Engine.VPT.Table;
using VisualPinball.Unity.VPT.Ball;
using VisualPinball.Unity.VPT.Flipper;
using VisualPinball.Unity.VPT.Kicker;
using VisualPinball.Unity.VPT.Table;

namespace VisualPinball.Unity.Game
{
	public class Player : MonoBehaviour
	{
		private readonly TableApi _tableApi = new TableApi();

		private readonly Dictionary<int, FlipperApi> _flippers = new Dictionary<int, FlipperApi>();
		private FlipperApi Flipper(int entityIndex) => _flippers.Values.FirstOrDefault(f => f.Entity.Index == entityIndex);

		//public static StreamWriter DebugLog;

		private Table _table;
		private EntityManager _manager;
		private BallManager _ballManager;

		public void RegisterFlipper(Flipper flipper, Entity entity)
		{
			var flipperApi = new FlipperApi(flipper, entity, this);
			_tableApi.Flippers[flipper.Name] = flipperApi;
			_flippers[entity.Index] = flipperApi;
			World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<FlipperVelocitySystem>().OnRotated +=
				(sender, e) => flipperApi.HandleEvent(e);
		}

		public void RegisterKicker(Kicker kicker, Entity entity)
		{
			var kickerApi = new KickerApi(kicker, entity, this);
			_tableApi.Kickers[kicker.Name] = kickerApi;
		}

		public BallApi CreateBall(IBallCreationPosition ballCreator, float radius = 25, float mass = 1)
		{
			// var data = new BallData(radius, mass, _table.Data.DefaultBulbIntensityScaleOnBall);
			// const ballId = Ball.idCounter++;
			// const state = BallState.claim(`Ball${ballId}`, ballCreator.getBallCreationPosition(this.table));
			// state.pos.z += data.radius;
			//
			// const ball = new Ball(ballId, data, state, ballCreator.getBallCreationVelocity(this.table), player, this.table);
			//
			// ballCreator.onBallCreated(this, ball);
			//
			// this.balls.push(ball);
			// this.movers.push(ball.getMover()); // balls are always added separately to this list!
			//
			// this.hitObjectsDynamic.push(ball.hit);
			// this.hitOcTreeDynamic.fillFromVector(this.hitObjectsDynamic);
			//
			// this.currentStates[ball.getName()] = ball.getState();
			// this.previousStates[ball.getName()] = ball.getState().clone();
			// this.emit('ballCreated', ball);
			// return ball;
			return null;
		}

		private void Awake()
		{
			var tableComponent = gameObject.GetComponent<TableBehavior>();
			_ballManager = new BallManager();
			_table = tableComponent.CreateTable();
			_manager = World.DefaultGameObjectInjectionWorld.EntityManager;

			//DebugLog = File.CreateText("flipper.log");
		}

		private void Start()
		{
			// bootstrap table script(s)
			var tableScripts = GetComponents<VisualPinballScript>();
			foreach (var tableScript in tableScripts) {
				tableScript.OnAwake(_tableApi);
			}

			// trigger init events now
			foreach (var i in _tableApi.Initializables) {
				i.Init();
			}
		}

		private void Update()
		{
			// flippers will be handled via script later, but until scripting works, do it here.
			if (Input.GetKeyDown("left shift")) {
				_tableApi.Flipper("LeftFlipper")?.RotateToEnd();
			}
			if (Input.GetKeyUp("left shift")) {
				_tableApi.Flipper("LeftFlipper")?.RotateToStart();
			}
			if (Input.GetKeyDown("right shift")) {
				_tableApi.Flipper("RightFlipper")?.RotateToEnd();
			}
			if (Input.GetKeyUp("right shift")) {
				_tableApi.Flipper("RightFlipper")?.RotateToStart();
			}
		}

		private void OnDestroy()
		{
			//DebugLog.Dispose();
		}
	}
}
