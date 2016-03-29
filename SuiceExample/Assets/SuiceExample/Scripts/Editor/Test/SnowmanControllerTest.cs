using DTools.Suice;
using NSubstitute;
using NUnit.Framework;
using SuiceExample.Factory;
using SuiceExample.Snowman;
using UnityEngine;
using UnitySuiceCommons.Resource;

namespace SuiceExample.Test
{
    /// <summary>
    /// This example test does not cover all test cases due to limitations of testing monobehaviours.
    /// I would need to create an interface that override all Monobehaviour functionality in unity.  Maybe I'll do this next time :).
    /// 
    /// @author DisTurBinG
    /// </summary>
    [TestFixture]
    public class SnowmanControllerTest
    {
        private ISnowmanMoveComponent snowmanMoveComponent;
        private ISnowmanPoolManager snowmanPoolManager;
        private IUnityResources unityResources;

        private SnowmanController snowmanController;


        private GameObject prefabTemplate;

        [SetUp]
        public void Setup()
        {
            prefabTemplate = new GameObject("Snowman");

            unityResources = Substitute.For<IUnityResources>();
            snowmanPoolManager = Substitute.For<ISnowmanPoolManager>();
            snowmanMoveComponent = Substitute.For<ISnowmanMoveComponent>();

            unityResources.Load(SnowmanController.SNOWMAN_ASSET_PATH, typeof(GameObject)).Returns(prefabTemplate);
            unityResources.Instantiate(prefabTemplate).Returns(prefabTemplate);
            
            snowmanController = new SnowmanController(snowmanPoolManager, unityResources);
            snowmanController.Initialize();

            var prop = snowmanController.GetType().GetField("moveComponent", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            prop.SetValue(snowmanController, snowmanMoveComponent);
        }

        [TearDown]
        public void Cleanup()
        {
            Object.DestroyImmediate(prefabTemplate);
        }

        [Test]
        public void DestroyWhileMovingTest()
        {
            snowmanMoveComponent.IsMoving().Returns(true);
            Assert.Throws<SnowmanController.CannotDestroyWhileMovingException>(() => 
                snowmanController.Destroy());
        }

        [Test]
        public void SuccessfulDestroyTest()
        {
            snowmanMoveComponent.IsMoving().Returns(false);

            Assert.DoesNotThrow(() => snowmanController.Destroy());
            snowmanPoolManager.Received(1).ReturnToPool(snowmanController);
        }

        [Test]
        public void TestSnowmanMovement()
        {
            snowmanController.MoveToRandomPosition(5);

            snowmanMoveComponent.Received(1).WalkToPosition(Arg.Is<Vector3>(targetPosition => 
                targetPosition.x >= SnowmanController.MIN_TARGET_MOVE_RANGE.x && targetPosition.x <= SnowmanController.MAX_TARGET_MOVE_RANGE.x &&
                targetPosition.x >= SnowmanController.MIN_TARGET_MOVE_RANGE.y && targetPosition.x <= SnowmanController.MAX_TARGET_MOVE_RANGE.y &&
                targetPosition.x >= SnowmanController.MIN_TARGET_MOVE_RANGE.z && targetPosition.x <= SnowmanController.MAX_TARGET_MOVE_RANGE.z), 5);
        }
    }
}