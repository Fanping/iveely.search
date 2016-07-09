package com.iveely.search.api.controller;

import com.iveely.framework.index.QueryExecutor;
import com.iveely.framework.index.StoredDocument;
import com.iveely.search.api.conifg.Configurate;

import org.apache.lucene.queryparser.classic.ParseException;
import org.springframework.stereotype.Controller;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RequestMethod;
import org.springframework.web.bind.annotation.RequestParam;
import org.springframework.web.bind.annotation.ResponseBody;

import java.io.IOException;
import java.util.List;

import io.swagger.annotations.ApiOperation;

/**
 * @author liufanping (liufanping@iveely.com)
 */
@Controller
@RequestMapping("/api/0.9.1" )
public class QueryController {

  private QueryExecutor executor;


  @ResponseBody
  @RequestMapping(value = "/query", method = RequestMethod.GET)
  @ApiOperation(value = "Query method", notes = "Submit query to get articles" +
      "." )
  public List<StoredDocument> doQuery(@RequestParam String queryField,
                                      @RequestParam String[] respFields,
                                      @RequestParam String keywords,
                                      @RequestParam int totalHits)
      throws IOException, ParseException {
    if (this.executor == null) {
      this.executor = new QueryExecutor(Configurate.indexPath);
    }
    return this.executor.find(
        queryField,
        respFields,
        keywords,
        totalHits);
  }
}
