module.exports=(function(){

  var fs=require('fs');
  var crypto=require('crypto');

  var Tagify=function(index,root){
    if (!fs.existsSync(index))
      fs.writeFileSync(index,JSON.stringify({current:[],lost:[]}));
    this._index=JSON.parse(fs.readFileSync(index));
    this._root=root;
    this._indexpath=index;
  }
  Tagify.prototype={
    _index:null,
    _saveTimer:null,
    
    _indexSort:function(index){
      return index.sort(function(a,b){return (a.path<b.path?-1:(a.path>b.path?1:0))})
    },
    _indexReduce:function(index){
      return this._indexSort(index).filter(function(s,i,arr){
        return (i==arr.length-1)||(s.path!=arr[i+1].path)
      });
    },
    _resolvePath:function(path,index){
      var list = index.filter(function(f){
        return f.path==path;
      });
      if (list.length==0)
        list = index.filter(function(f){
          var success = false;
          try {success=(fs.realpathSync(path)==fs.realpathSync(f.path))}
          catch (e) {success=false}
          return success;
        });
      if (list.length>1)
        console.error("Ambiguos file path, index probably damaged: ",list.map(function(f){return f.path}).join(','));
      if (list.length==0)
        return null;
      return list[0];
    },
    getSha:function(data){
      var sha=crypto.createHmac('sha512','asdasdasd');
      sha.update(data);
      return sha.digest().toString('hex');
    },
    scan:function(path){
      var _this=this,
          dig=function(path){
            var files=fs.readdirSync(path).map(function(p){return path+'/'+p});
            return [].concat.apply([],[files.filter(function(f){
                  return !fs.statSync(f).isDirectory();
                }).map(function(p){
                  return {path:p,tags:[],sha:null}
                })
              ].concat(files.filter(function(f){
                  return fs.statSync(f).isDirectory();
                }).map(function(f){
                  return dig(f).concat({path:f,tags:[],sha:null});
                })
              )
            )
          }
      return dig(path);
    },
    reload:function(){
      var _this=this,
          oldindex = this._indexSort(this._index.current),
          newindex = this._indexSort(this.scan(this._root)),
          known = [],
          newfiles = [],
          lostfiles = [];
          
      for (var i = 0 , j = 0 ; i<oldindex.length && j<newindex.length;){
        if (oldindex[i].path>newindex[j].path)
          newfiles.push(newindex[j++]);
        else if (oldindex[i].path<newindex[j].path)
          lostfiles.push(oldindex[i++]);
        else {
          known.push(oldindex[i++]);
          j++;
        }
      }
      [].push.apply(newfiles,newindex.slice(j));
      [].push.apply(lostfiles,oldindex.slice(i));
      
      for (var i = 0 ; i < newfiles.length ; i++){
        if (fs.statSync(newfiles[i].path).isDirectory()) continue;
        console.error('Computing sha for '+i+' out of '+newfiles.length+' files ('+newfiles[i].path+')');
        newfiles[i].sha=this.getSha(fs.readFileSync(newfiles[i].path));
      }
      this._index.current=this._indexSort(known.concat(newfiles));
      this._index.lostfiles=this._indexReduce(this._index.lost.concat(lostfiles));
      console.error('New files: '+newfiles.length+' lost files: '+lostfiles.length+' known files '+known.length);
    },
    save:function(){
      fs.writeFileSync(this._indexpath,JSON.stringify(this._index,null,2));
    },
    scheduleSave:function(){
      var _this = this;
      clearTimeout(this._saveTimer);
      this._saveTimer=setTimeout(function(){_this.save();console.error('Saved...');}, 2000);
    },
    
    
    addTags:function(path,tags){
      tags=tags.map(function(t){return t.toLowerCase().trim()});
      var resolvedFile = this._resolvePath(path,this._index.current);
      if (!resolvedFile) throw new Error("File not found");
      tags.forEach(function(t){
        if (resolvedFile.tags.indexOf(t)==-1)
          resolvedFile.tags.push(t);
      });
    },
    removeTags:function(path,tags){
      tags=tags.map(function(t){return t.toLowerCase().trim()});
      var resolvedFile = this._resolvePath(path,this._index.current);
      if (!resolvedFile) throw new Error("File not found");
      tags.forEach(function(t){
        var ind = resolvedFile.tags.indexOf(t);
        if (ind!=-1)
          resolvedFile.tags.splice(ind,1);
      });
    },
    getTags:function(path){
      var resolvedFile = this._resolvePath(path,this._index.current);
      if (!resolvedFile) throw new Error("File not found");
      return resolvedFile.tags;
    },
    searchTags:function(tags){
      tags=tags.map(function(t){return t.toLowerCase().trim()});
      return this._index.current.filter(function(f){
        return tags.filter(function(t){return f.tags.indexOf(t)!=-1}).length
      }).map(function(f){return f.path});
    }
  }
  return Tagify;
})()