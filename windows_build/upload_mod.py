#!/usr/bin/python3

""" 
    upload_mod.py
    
    Web / html5 builds of a game cannot typically read from / scan files on disk as desktop builds can.
    Instead, they must retrieve / download necessary files from a web server.
    To do this, the build must have a way to locate relevant files on the web server without scanning.
    We need a special, "always-there" manifest file to tell our engine where to locate other resource files.
"""

import pathlib
import json
import os
import uuid
import webbrowser

def get_developer_id():
    result = ""
    if os.path.isfile('./developer_id'):
        with open('./developer_id') as f:
            result = f.read()
            
    if result == None or result == "":
        result = str(uuid.uuid4())
        with open('./developer_id', 'w') as f:
            f.write(result)
    
    return result
    

def compile_resource_manifest():
    resource_folder = './resources'
    resource_types = [name for name in os.listdir(resource_folder) if os.path.isdir(os.path.join(resource_folder, name))]

    result = {}
    for resource_type in resource_types:
        result[resource_type] = [str(a.as_posix()) for a in pathlib.Path('resources/' + resource_type + '/').glob('**/*.*')]

    return result


def upload(mod_manifest):
    # TODO : 
    # (1) upload files to AWS lambda.
    # (2) lambada store files in proper S3 bucket.
    # (3) lambda updates "mod_gallery.json"
    pass
    


def main():
    developer_id = get_developer_id()
    print("\ncompiling mod manifest for developer [" + developer_id + "]\n")
    mod_manifest = compile_resource_manifest()
    
    print(mod_manifest)
    upload(mod_manifest)
    
    webbrowser.open('http://arborinteractive.com/hw2/game.html?developer_id=eba989fc-491e-4bc4-8a24-8377c41e0a8e')

if __name__ == "__main__":
    main()
