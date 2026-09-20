#nullable enable
#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
namespace GameCanvas.Tests
{
    public class NetworkTest
    {
        HttpListener server = null!;
        string url = "";
        InsecureHttpOption original;
        byte[] png = null!;
        [SetUp] public void Setup()
        {
            original = PlayerSettings.insecureHttpOption; PlayerSettings.insecureHttpOption = InsecureHttpOption.AlwaysAllowed;
            var texture = new Texture2D(4,3); png=texture.EncodeToPNG(); UnityEngine.Object.DestroyImmediate(texture);
            var socket = new TcpListener(IPAddress.Loopback,0); socket.Start(); int port=((IPEndPoint)socket.LocalEndpoint).Port; socket.Stop();
            url=$"http://127.0.0.1:{port}/"; server=new HttpListener(); server.Prefixes.Add(url); server.Start();
            _=Serve();
        }
        async Task Serve()
        {
            try { while(server.IsListening) { var ctx=await server.GetContextAsync(); _=Respond(ctx); } }
            catch(ObjectDisposedException) { } catch(HttpListenerException) { }
        }
        async Task Respond(HttpListenerContext ctx)
        {
            try
            {
                var path=ctx.Request.Url!.AbsolutePath;
                if(path=="/slow") await Task.Delay(1000);
                byte[] data=Encoding.UTF8.GetBytes("通信成功 日本語");
                if(path=="/redirect") { ctx.Response.StatusCode=302; ctx.Response.RedirectLocation=url+"/text"; data=Array.Empty<byte>(); }
                if(path=="/image") {data=png;ctx.Response.ContentType="image/png";}
                if(path=="/sound") { data=Wave();ctx.Response.ContentType="audio/wav"; }
                if(path=="/missing") ctx.Response.StatusCode=404;
                if(path=="/empty") {ctx.Response.StatusCode=204;data=Array.Empty<byte>();}
                if(path=="/echo") {using var reader=new StreamReader(ctx.Request.InputStream,Encoding.UTF8);data=Encoding.UTF8.GetBytes(await reader.ReadToEndAsync());}
                ctx.Response.ContentLength64=data.Length;
                await ctx.Response.OutputStream.WriteAsync(data,0,data.Length); ctx.Response.Close();
            }
            catch(ObjectDisposedException) { } catch(HttpListenerException) { } catch(IOException) { }
        }
        [TearDown] public void Teardown() {server?.Close();PlayerSettings.insecureHttpOption=original;}
        static byte[] Wave()
        {
            using var stream=new MemoryStream();using var writer=new BinaryWriter(stream);
            const int samples=800;
            writer.Write(Encoding.ASCII.GetBytes("RIFF"));writer.Write(36+samples*2);writer.Write(Encoding.ASCII.GetBytes("WAVEfmt "));
            writer.Write(16);writer.Write((short)1);writer.Write((short)1);writer.Write(8000);writer.Write(16000);
            writer.Write((short)2);writer.Write((short)16);writer.Write(Encoding.ASCII.GetBytes("data"));writer.Write(samples*2);
            for(int i=0;i<samples;i++)writer.Write((short)(Math.Sin(i*Math.PI*2*440/8000)*1000));
            return stream.ToArray();
        }
        IEnumerator Complete(GcNetworkService service, params GcRequest[] requests)
        {
            double deadline=Time.realtimeSinceStartupAsDouble+10;
            while(true)
            {
                service.Tick(); bool done=true;for(int i=0;i<requests.Length;i++)done &= requests[i].IsDone;
                if(done)yield break;
                Assert.That(Time.realtimeSinceStartupAsDouble,Is.LessThan(deadline));yield return null;
            }
        }
        [UnityTest] public IEnumerator RequestsHandleRedirectsTypesErrorsAndOwnership()
        {
            using var service=new GcNetworkService();
            using var text=service.GetText(url+"redirect");
            using var empty=service.GetText(url+"empty");
            using var image=service.GetImage(url+"image");
            using var sameUrlAsText=service.GetText(url+"image");
            using var missing=service.GetText(url+"missing");
            using var post=service.PostText(url+"echo","{\"日本語\":42}","application/json");
            using var form=service.PostForm(url+"echo",new Dictionary<string,string>{{"名 前","a&b=日本語"}});
            yield return Complete(service,text,empty,image,sameUrlAsText,missing,post,form);
            Assert.That(text.Status,Is.EqualTo(GcRequestState.Succeeded));Assert.That(text.Url,Is.EqualTo(url+"redirect"));
            Assert.That(text.Text,Is.EqualTo("通信成功 日本語"));Assert.That(empty.Status,Is.EqualTo(GcRequestState.Succeeded));Assert.That(empty.ResponseCode,Is.EqualTo(204));
            Assert.That(image.Status,Is.EqualTo(GcRequestState.Succeeded));Assert.That(image.Width,Is.EqualTo(4));Assert.That(image.Height,Is.EqualTo(3));
            Assert.That(sameUrlAsText.Status,Is.EqualTo(GcRequestState.Succeeded));
            Assert.That(missing.ErrorCode,Is.EqualTo("GC-NETWORK-HTTP"));Assert.That(missing.ResponseCode,Is.EqualTo(404));
            Assert.That(post.Text,Is.EqualTo("{\"日本語\":42}"));Assert.That(form.Text,Is.EqualTo("%E5%90%8D%20%E5%89%8D=a%26b%3D%E6%97%A5%E6%9C%AC%E8%AA%9E"));
            var native=image.Texture;image.Dispose();yield return null;
            Assert.That(native==null,Is.True);Assert.That(image.Width,Is.Zero);
            long before=GC.GetAllocatedBytesForCurrentThread();for(int i=0;i<1000;i++)service.Tick();long bytes=GC.GetAllocatedBytesForCurrentThread()-before;
            Assert.That(bytes,Is.Zero);
        }
        [UnityTest] public IEnumerator CancelTimeoutDisposeAndRetryHaveIndependentLifetimes()
        {
            double clock=0;using var service=new GcNetworkService(()=>clock);
            var cancelled=service.GetText(url+"slow");cancelled.Cancel();
            using var timed=service.GetText(url+"slow",timeoutSeconds:1);clock=2;service.Tick();
            Assert.That(timed.Status,Is.EqualTo(GcRequestState.TimedOut));Assert.That(cancelled.Status,Is.EqualTo(GcRequestState.Cancelled));
            using var retry=service.GetText(url+"text");yield return Complete(service,retry);
            Assert.That(retry.Status,Is.EqualTo(GcRequestState.Succeeded));
            var pending=service.GetText(url+"slow");service.CancelAll();Assert.That(pending.Status,Is.EqualTo(GcRequestState.Cancelled));Assert.That(retry.Text,Is.Not.Empty);
            service.Dispose();Assert.That(pending.Status,Is.EqualTo(GcRequestState.Disposed));Assert.That(retry.Text,Is.Empty);
            Assert.Throws<ObjectDisposedException>(()=>service.GetText(url));
        }
        [UnityTest] public IEnumerator DownloadedSoundRetainsClipUntilDisposed()
        {
            using var service=new GcNetworkService();using var sound=service.GetSound(url+"sound",GcSoundFormat.Wav);
            Assert.That(sound.Duration,Is.Zero);
            yield return Complete(service,sound);
            Assert.That(sound.Status,Is.EqualTo(GcRequestState.Succeeded));
            Assert.That(sound.Duration,Is.EqualTo(.1f).Within(.001f));
            var clip=sound.Clip;Assert.That(clip,Is.Not.Null);
            Assert.That(clip!.channels,Is.EqualTo(1));Assert.That(clip.frequency,Is.EqualTo(8000));
            sound.Dispose();yield return null;
            Assert.That(clip==null,Is.True);Assert.That(sound.Duration,Is.Zero);
        }
        [Test] public void InvalidInputsDoNotStartRequests()
        {
            using var service=new GcNetworkService();
            Assert.Throws<ArgumentException>(()=>service.GetText("file:///tmp/test"));
            foreach(double timeout in new[]{0,-1,double.NaN,double.PositiveInfinity})
                Assert.Throws<ArgumentOutOfRangeException>(()=>service.GetText(url,timeout));
            Assert.Throws<ArgumentOutOfRangeException>(()=>service.GetSound(url,(GcSoundFormat)99));
            Assert.Throws<ArgumentNullException>(()=>service.PostText(url,null!));
        }
    }
}
#endif
