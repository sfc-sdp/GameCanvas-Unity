/*------------------------------------------------------------*/
// <summary>GameCanvas for Unity</summary>
// <author>Seibe TAKAHASHI</author>
// <remarks>
// (c) 2015-2020 Smart Device Programming.
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php
// </remarks>
/*------------------------------------------------------------*/
namespace GameCanvas
{
    public interface IScene
    {
        /// <summary>
        /// シーンの描画処理
        /// </summary>
        /// <remarks>
        /// 毎フレーム（計算より後に）GameCanvasにより自動的に呼び出されます
        /// </remarks>
        void DrawScene();

        /// <summary>
        /// シーンの開始処理
        /// </summary>
        /// <remarks>
        /// シーン遷移時にGameCanvasにより自動的に呼び出されます
        /// </remarks>
        /// <param name="state"><see cref="ISceneManagementEx.ChangeScene"/>を通じて渡される任意の値。未設定の場合はnull</param>
        void EnterScene(object state);

        /// <summary>
        /// シーンの終了処理
        /// </summary>
        /// <remarks>
        /// シーン遷移時にGameCanvasにより自動的に呼び出されます
        /// </remarks>
        void LeaveScene();

        /// <summary>
        /// シーンの中断処理
        /// </summary>
        /// <remarks>
        /// アプリが一時停止する直前にGameCanvasにより自動的に呼び出されます
        /// </remarks>
        void PauseScene();

        /// <summary>
        /// シーンの再開処理
        /// </summary>
        /// <remarks>
        /// アプリが再開する直前にGameCanvasにより自動的に呼び出されます
        /// </remarks>
        void ResumeScene();

        /// <summary>
        /// シーンの計算処理
        /// </summary>
        /// <remarks>
        /// 毎フレーム（描画より前に）GameCanvasにより自動的に呼び出されます
        /// </remarks>
        void UpdateScene();
    }

    public interface ISceneManagement
    {
        /// <summary>
        /// 指定したアクターをシーンに登録します
        /// </summary>
        /// <param name="actor">登録するアクター</param>
        void AddActor(in GcActor actor);

        /// <summary>
        /// 指定したアクターを生成し、シーンに登録します
        /// </summary>
        /// <typeparam name="T">生成・登録するアクターの型</typeparam>
        /// <returns>登録したアクター</returns>
        T CreateActor<T>() where T : GcActor, new();

        /// <summary>
        /// シーンに登録されているアクターの総数を取得します
        /// </summary>
        /// <returns>アクターの数</returns>
        int GetActorCount();

        /// <summary>
        /// シーンに登録されているアクターのうち、指定した型のものが幾つあるか取得します
        /// </summary>
        /// <typeparam name="T">取得するアクターの型</typeparam>
        /// <returns>アクターの数</returns>
        int GetActorCount<T>() where T : GcActor;

        /// <summary>
        /// シーンに登録されているすべてのアクターを登録解除します
        /// </summary>
        void RemoveActorAll();

        /// <summary>
        /// シーンに登録されているアクターを 1つだけ取得します
        /// </summary>
        /// <typeparam name="T">取得するアクターの型</typeparam>
        /// <param name="i">取得するアクターのインデックス（0以上<see cref="GetActorCount"/>未満）</param>
        /// <param name="actor">取得できたアクター</param>
        /// <returns>取得できたかどうか</returns>
        bool TryGetActor(in int i, out GcActor actor);

        /// <summary>
        /// シーンに登録されているアクターを 1つだけ取得します
        /// </summary>
        /// <typeparam name="T">取得するアクターの型</typeparam>
        /// <param name="i">取得するアクターのインデックス（0以上<see cref="GetActorCount{T}"/>未満）</param>
        /// <param name="actor">取得できたアクター</param>
        /// <returns>取得できたかどうか</returns>
        bool TryGetActor<T>(in int i, out T actor) where T : GcActor;

        /// <summary>
        /// シーンに登録されているアクターのうち、指定した型のものを取得します
        /// </summary>
        /// <typeparam name="T">取得するアクターの型</typeparam>
        /// <param name="list">取得したアクターのリスト</param>
        /// <returns>1つ以上 取得できたかどうか</returns>
        bool TryGetActorList<T>(out ReadOnlyActorList<T> list) where T : GcActor;

        /// <summary>
        /// 指定したアクターをシーンから登録解除します
        /// </summary>
        /// <param name="actor">登録解除するアクター</param>
        /// <returns>登録解除できたかどうか</returns>
        bool TryRemoveActor(in GcActor actor);
    }

    public interface ISceneManagementEx : ISceneManagement
    {
        /// <summary>
        /// シーンを切り替えます
        /// </summary>
        /// <remarks>
        /// - これまで有効だったシーンは、現在のフレームの最後に終了処理が実行されます<br />
        /// - これから有効になるシーンは、次のフレームの最初に開始処理が実行されます
        /// </remarks>
        /// <typeparam name="T">開始するシーンの型</typeparam>
        /// <param name="state">シーンの開始処理 (<see cref="IScene.EnterScene"/>) に引数として渡す任意の値</param>
        void ChangeScene<T>(object state = null) where T : GcScene;

        /// <summary>
        /// シーンに登録されているアクターのうち、1つだけ取得します
        /// </summary>
        /// <returns>取得できたアクター</returns>
        GcActor GetActor();

        /// <summary>
        /// シーンに登録されているアクターのうち、指定した型のものを1つだけ取得します
        /// </summary>
        /// <typeparam name="T">取得するアクターの型</typeparam>
        /// <returns>取得できたアクター</returns>
        T GetActor<T>() where T : GcActor;

        /// <summary>
        /// シーンに登録されているアクターのうち、指定した型のものを取得します
        /// </summary>
        /// <typeparam name="T">取得するアクターの型</typeparam>
        /// <returns>取得したアクターのリスト</returns>
        ReadOnlyActorList<T> GetActorList<T>() where T : GcActor;

        /// <summary>
        /// 新たなシーンを登録します
        /// </summary>
        /// <remarks>
        /// 登録したシーンは <see cref="ChangeScene"/> を呼び出すことで有効になります
        /// </remarks>
        /// <typeparam name="T">登録するシーンの型</typeparam>
        void RegisterScene<T>() where T : GcScene, new();

        /// <summary>
        /// 新たなシーンを登録します
        /// </summary>
        /// <remarks>
        /// 登録したシーンは <see cref="ChangeScene"/> を呼び出すことで有効になります
        /// </remarks>
        /// <param name="scene">登録するシーン</param>
        void RegisterScene(in GcScene scene);

        /// <summary>
        /// 指定したシーンをシーン一覧から削除します
        /// </summary>
        /// <remarks>
        /// もし指定したシーンが現在有効なシーンだった場合、フレームの最後にシーンの離脱処理が走ります
        /// </remarks>
        /// <typeparam name="T">削除するシーンの型</typeparam>
        void UnregisterScene<T>() where T : GcScene;

        /// <summary>
        /// 指定したシーンをシーン一覧から削除します
        /// </summary>
        /// <remarks>
        /// もし指定したシーンが現在有効なシーンだった場合、フレームの最後にシーンの離脱処理が走ります
        /// </remarks>
        /// <param name="scene">削除するシーン</param>
        void UnregisterScene(in GcScene scene);
    }
}
