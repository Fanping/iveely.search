package com.iveely.search.api.controller;

import org.springframework.stereotype.Controller;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RequestMethod;
import org.springframework.web.bind.annotation.RequestParam;
import org.springframework.web.bind.annotation.ResponseBody;

import io.swagger.annotations.ApiOperation;
import io.swagger.annotations.ApiParam;

@Controller
@RequestMapping("/api" )
public class ExampleController {

  @ResponseBody
  @RequestMapping(value = "/example", method = RequestMethod.POST)
  @ApiOperation(value = "Example method", notes = "this is an example test mehtod." )
  public String example(
      @ApiParam(required = true, name = "name", value = "Name" )
      @RequestParam(name = "name" ) String name) {
    return "Hello," + name;
  }
}