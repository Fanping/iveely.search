package com.iveely.search.api;

import static com.google.common.base.Predicates.or;
import static springfox.documentation.builders.PathSelectors.regex;

import org.springframework.context.annotation.Bean;
import org.springframework.context.annotation.Configuration;
import org.springframework.web.context.request.async.DeferredResult;

import springfox.documentation.service.ApiInfo;
import springfox.documentation.spi.DocumentationType;
import springfox.documentation.spring.web.plugins.Docket;
import springfox.documentation.swagger2.annotations.EnableSwagger2;

/**
 * SwaggerConfig
 */
@Configuration
@EnableSwagger2
public class SwaggerConfig {

  @Bean
  public Docket getApi() {
    return new Docket(DocumentationType.SWAGGER_2)
        .groupName("iveely.search")
        .genericModelSubstitutes(DeferredResult.class)
        .useDefaultResponseMessages(false)
        .forCodeGeneration(false)
        .pathMapping("/")
        .select()
        .paths(or(regex("/api/.*")))
        .build()
        .apiInfo(getApiInfo());
  }

  private ApiInfo getApiInfo() {
    ApiInfo apiInfo = new ApiInfo("Iveely Search Engine API",
        "Iveely is an open source search engine, including crawler, " +
            "distributed computing framework and so on.",
        "0.9.1",
        "http://github.com/fanping/iveely.search",
        "liufanping@iveely.com",
        "The Apache License, Version 2.0",
        "http://www.apache.org/licenses/LICENSE-2.0.html"
    );
    return apiInfo;
  }
}