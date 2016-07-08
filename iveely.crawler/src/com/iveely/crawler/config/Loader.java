package com.iveely.crawler.config;

import com.iveely.framework.text.JSONUtil;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import java.io.File;
import java.util.ArrayList;
import java.util.Arrays;
import java.util.List;

public class Loader {
  public static final Logger logger = LoggerFactory.getLogger(
      Loader.class);

  public static List<Seed> fromLocal() {
    List<Seed> configs = new ArrayList<>();

    // 1. Check file exist.
    final File confPathFile = new File("conf/sites/");
    if (!confPathFile.exists()) {
      return configs;
    }

    // 2. Get task list.
    final File[] taskFiles = confPathFile.listFiles();
    if (taskFiles == null) {
      return configs;
    }

    Arrays.stream(taskFiles)
        .forEach(file -> {
          if (file.getName().endsWith(".json")) {
            Seed seed =
                JSONUtil.fromFile(file, Seed.class);
            configs.add(seed);
          }
        });
    return configs;
  }
}
