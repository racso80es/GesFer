import sys

def main():
    with open('src/Product/Back/application/Handlers/ArticleFamilies/DeleteArticleFamilyCommandHandler.cs', 'r') as f:
        print("DELETE HANDLER:\n", f.read()[:500])

if __name__ == "__main__":
    main()
