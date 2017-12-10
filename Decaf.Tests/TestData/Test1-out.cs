// https://stackoverflow.com/a/559781/4077294

/*
 Licensed to the Apache Software Foundation (ASF) under one
or more contributor license agreements.  See the NOTICE file
distributed with this work for additional information
regarding copyright ownership.  The ASF licenses this file
to you under the Apache License, Version 2.0 (the
"License"); you may not use this file except in compliance
with the License.  You may obtain a copy of the License at

http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing,
software distributed under the License is distributed on an
"AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
KIND, either express or implied.  See the License for the
specific language governing permissions and limitations
under the License.
*/
using Java.IO;

namespace Com.Wilson.Android.Library
{
    public class DrawableManager
    {
        private readonly IDictionary<string, Drawable> drawableMap;

        public DrawableManager()
        {
            drawableMap = new Dictionary<String, Drawable>();
        }

        public Drawable FetchDrawable(string urlString)
        {
            if (drawableMap.ContainsKey(urlString))
            {
                return drawableMap.Get(urlString);
            }

            Log.D(this.Class.SimpleName, "image url:" + urlString);
            try
            {
                InputStream is = Fetch(urlString);
                Drawable drawable = Drawable.CreateFromStream(is, "src");


                if (drawable != null)
                {
                    drawableMap.Put(urlString, drawable);
                    Log.D(this.Class.SimpleName, "got a thumbnail drawable: " + drawable.Bounds + ", "
                            + drawable.IntrinsicHeight + "," + drawable.IntrinsicWidth + ", "
                            + drawable.MinimumHeight + "," + drawable.MinimumWidth);
                }
                else
                {
                    Log.W(this.Class.SimpleName, "could not get thumbnail");
                }

                return drawable;
            }
            catch (MalformedURLException e)
            {
                Log.E(this.Class.SimpleName, "fetchDrawable failed", e);
                return null;
            }
            catch (IOException e)
            {
                Log.E(this.Class.SimpleName, "fetchDrawable failed", e);
                return null;
            }
        }

        public void FetchDrawableOnThread(final string urlString, final ImageView imageView)
        {
            if (drawableMap.ContainsKey(urlString))
            {
                imageView.ImageDrawable = drawableMap.Get(urlString);
            }

            final Handler handler = new Handler()
            {
                public override void HandleMessage(Message message)
                {
                    imageView.setImageDrawable((Drawable)message.obj);
                }
            };

            Thread thread = new Thread()
            {
                public override void Run()
                {
                    //TODO : set imageView to a "pending" image
                    Drawable drawable = FetchDrawable(urlString);
                    Message message = handler.ObtainMessage(1, drawable);
                    handler.SendMessage(message);
                }
            };
            thread.Start();
        }

        private InputStream Fetch(string urlString)
        {
            DefaultHttpClient httpClient = new DefaultHttpClient();
            HttpGet request = new HttpGet(urlString);
            HttpResponse response = httpClient.Execute(request);
            return response.Entity.Content;
        }
    }
}