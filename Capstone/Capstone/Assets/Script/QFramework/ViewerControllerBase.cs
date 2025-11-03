using UnityEngine;
using QFramework;

namespace SkateGame
{
    /// <summary>
    /// 实时检测控制器基类
    /// 继承MonoBehaviour和IController，提供QFramework的所有能力
    /// 所有实时检测层都应该继承此类
    /// </summary>
    public abstract class ViewerControllerBase : MonoBehaviour, IController, ICanSendEvent, ICanGetModel, ICanRegisterEvent, ICanSendCommand, ICanGetSystem, ICanSendQuery, ICanGetUtility, ICanSetArchitecture
    {
        [Header("基础设置")]
        public bool isActive = true;
        public float updateInterval = 0.016f; // 60fps
        
        protected float lastUpdateTime;
        
        protected virtual void Start()
        {
            InitializeController();
        }
        
        protected virtual void Update()
        {
            if (!isActive || this == null || !gameObject) return;

            // 执行实时检测
            OnRealTimeUpdate();
            // 控制更新频率
            if (Time.time - lastUpdateTime < updateInterval) return;
            lastUpdateTime = Time.time;
            
          
        }

        
        /// <summary>
        /// 初始化控制器
        /// </summary>
        protected virtual void InitializeController()
        {
            // 子类重写此方法进行初始化
        }
        
        /// <summary>
        /// 实时更新方法 - 子类必须实现
        /// 只负责检测，不包含业务逻辑
        /// </summary>
        protected abstract void OnRealTimeUpdate();
        
        /// <summary>
        /// 激活控制器
        /// </summary>
        public virtual void Activate()
        {
            isActive = true;
        }
        
        /// <summary>
        /// 停用控制器
        /// </summary>
        public virtual void Deactivate()
        {
            isActive = false;
        }
        
        /// <summary>
        /// 设置更新频率
        /// </summary>
        public void SetUpdateInterval(float interval)
        {
            updateInterval = Mathf.Max(0.001f, interval);
        }
        
        /// <summary>
        /// 获取架构接口 - QFramework要求
        /// </summary>
        public IArchitecture GetArchitecture()
        {
            return GameApp.Interface;
        }
        
        /// <summary>
        /// 设置架构接口 - QFramework要求
        /// </summary>
        public void SetArchitecture(IArchitecture architecture)
        {
            // 对于Controller，通常不需要设置架构，因为使用GameApp.Interface
        }
        
        /// <summary>
        /// 销毁时的清理工作
        /// </summary>
        protected virtual void OnDestroy()
        {
            // 场景切换时停止更新
            isActive = false;
            // 子类可以重写此方法进行清理
        }
        
        /// <summary>
        /// 禁用时停止更新
        /// </summary>
        protected virtual void OnDisable()
        {
            isActive = false;
        }
        
        /// <summary>
        /// 启用时恢复更新
        /// </summary>
        protected virtual void OnEnable()
        {
            if (this != null && gameObject != null)
            {
                isActive = true;
            }
        }
    }
}
