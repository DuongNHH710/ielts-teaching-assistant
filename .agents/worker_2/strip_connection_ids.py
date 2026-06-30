import re

files = [
    r"d:\Project\ielts-teaching-assistant\src\IeltsTeachingAssistant\Views\SpeakingEvaluationPage.xaml",
    r"d:\Project\ielts-teaching-assistant\src\IeltsTeachingAssistant\Views\WritingEvaluationPage.xaml"
]

for filepath in files:
    with open(filepath, "r", encoding="utf-8") as f:
        content = f.read()
    
    # Remove x:ConnectionId='...' or x:ConnectionId="..."
    cleaned = re.sub(r"\s*x:ConnectionId=['\"][^'\"]*['\"]", "", content)
    
    with open(filepath, "w", encoding="utf-8") as f:
        f.write(cleaned)
    print(f"Cleaned {filepath}")
