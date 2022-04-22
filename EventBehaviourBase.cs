using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// イベント管理型コンポーネント基底クラス
/// </summary>
public abstract class EventBehaviourBase : MonoBehaviour
{
    Dispose dispose;

    /// <summary>
    /// イベント発行型を生成
    /// </summary>
    /// <typeparam name="T">イベントの型</typeparam>
    /// <returns>イベント実行クラス</returns>
    protected IExecuter<T> Regist<T>() where T : unmanaged
    {
        return EventController.Regist<T>();
    }

    /// <summary>
    /// イベントを登録
    /// </summary>
    /// <typeparam name="T">イベントの型</typeparam>
    /// <param name="act">実行する関数</param>
    protected void Subscribe<T>(System.Action<T> act) where T : unmanaged
    {
        dispose += EventController.AddQuery<T>(act);
    }

    /// <summary>
    /// 破棄時実行 オーバーライドする場合必ず呼び出してください
    /// </summary>
    protected virtual void OnDestroy()
    {
        dispose?.Invoke();
    }
}
