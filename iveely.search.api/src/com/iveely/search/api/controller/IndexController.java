package com.iveely.search.api.controller;

import com.iveely.framework.index.IndexBuilder;
import com.iveely.framework.index.StoredDocument;
import com.iveely.search.api.conifg.Configurate;

import org.springframework.stereotype.Controller;
import org.springframework.web.bind.annotation.RequestBody;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RequestMethod;
import org.springframework.web.bind.annotation.ResponseBody;

import java.io.IOException;

import io.swagger.annotations.ApiOperation;
import io.swagger.annotations.ApiParam;

/**
 * @author liufanping (liufanping@iveely.com)
 */
@Controller
@RequestMapping("/api/0.9.1" )
public class IndexController {

  private IndexBuilder indexBuilder;

  private static int count;

  public IndexController()  {
    count = 0;
  }

  @ResponseBody
  @RequestMapping(value = "/index", method = RequestMethod.POST)
  @ApiOperation(value = "Index method", notes = "Submit an article to index." )
  public void forArticle(
      @ApiParam(required = true, name = "article", value = "This is document " +
          "article." )
      @RequestBody StoredDocument document) throws IOException {
    if(indexBuilder == null){
      this.indexBuilder = new IndexBuilder(Configurate.indexPath);
    }
    indexBuilder.build(document.getStoreFields().get(0), document.getStoreFields());
    count++;
    if (count % 1 == 0) {
      this.indexBuilder.flush(false);
    }
  }
}
