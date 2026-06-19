import urllib.request
import json
import os

creds_path = os.path.expandvars(r'%LOCALAPPDATA%\..\Roaming\gcloud\application_default_credentials.json')

# We can use gcloud command to quickly get a fresh access token for testing
token = os.popen("gcloud auth print-access-token").read().strip()

project_id = 'gen-lang-client-0376290806'
region = 'us-central1'
model_id = 'gemini-2.5-flash' # Let's try the GA model

endpoint = f"https://{region}-aiplatform.googleapis.com/v1/projects/{project_id}/locations/{region}/publishers/google/models/{model_id}:generateContent"

payload = {
    "contents": [
        {"role": "user", "parts": [{"text": "Hello"}]}
    ]
}

req = urllib.request.Request(endpoint, data=json.dumps(payload).encode('utf-8'))
req.add_header('Authorization', f'Bearer {token}')
req.add_header('Content-Type', 'application/json')

try:
    with urllib.request.urlopen(req) as response:
        print("SUCCESS:", response.read().decode('utf-8'))
except urllib.error.HTTPError as e:
    print(f"HTTP ERROR: {e.code}")
    print(e.read().decode('utf-8'))
except Exception as e:
    print(f"ERROR: {e}")
