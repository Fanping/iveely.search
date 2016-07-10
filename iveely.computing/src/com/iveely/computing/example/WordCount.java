/*
 * To change this license header, choose License Headers in Project Properties.
 * To change this template file, choose Tools | Templates
 * and open the template in the editor.
 */
package com.iveely.computing.example;

import com.iveely.computing.api.DataTuple;
import com.iveely.computing.api.IInput;
import com.iveely.computing.api.IOutput;
import com.iveely.computing.api.StreamChannel;
import com.iveely.computing.api.Topology;
import com.iveely.computing.api.Topology.ExecuteType;
import com.iveely.computing.api.TopologySubmitter;
import com.iveely.computing.api.writable.IntWritable;
import com.iveely.computing.api.writable.StringWritable;

import java.io.IOException;
import java.util.HashMap;
import java.util.Map;
import java.util.Random;

/**
 * Word Count exmaple for iveely.computing.
 *
 * @author sea11510@mail.ustc.edu.cn
 */
public class WordCount {

  public static class WordInput extends IInput {

    private int MAX = 1000;

    @Override
    public void nextTuple(StreamChannel channel) {
      if (MAX < 1) {
        try {
          channel.emitEnd();
          return;
        } catch (Exception ex) {
          ex.printStackTrace();
        }
      }

      MAX--;
      final String[] words = new String[]{"iveely", "mike", "jackson", "golda", "bertels", "blue", "china",
          "pan", "qq", "baidu", "ABC", "desk", "pen", "music", "play", "mouse", "mac", "windows", "microsoft",
          "c++", "java"};
      channel.emit(new StringWritable(words[new Random().nextInt(words.length - 1)]), new IntWritable(1));
    }

    @Override
    public void toOutput(StreamChannel channel) {
      try {
        channel.addOutputTo(WordOutput.class);
      } catch (Exception e) {
        e.printStackTrace();
      }

    }
  }

  public static class WordOutput extends IOutput {

    private Map<String, Integer> map;
    int cnt;

    @Override
    public void start(HashMap<String, Object> conf) {
      this.map = new HashMap<>();
      cnt = 0;
    }

    @Override
    public void execute(DataTuple tuple, StreamChannel channel) {
      String word = ((StringWritable) tuple.getKey()).get();
      Integer defaultCount = ((IntWritable) tuple.getValue()).get();
      if (map.containsKey(word)) {
        int currentCount = map.get(word);
        map.put(word, defaultCount + currentCount);
      } else {
        map.put(word, defaultCount);
      }
      cnt++;
    }

    @Override
    public void end(HashMap<String, Object> conf) {
      Integer sum = 0;
      for (Map.Entry<String, Integer> entry : map.entrySet()) {
        sum += entry.getValue();
        System.out.println("Key = " + entry.getKey() + ", Value = " + entry.getValue());
      }
      System.out.println("Total sum:" + sum);
    }
  }

  public static void main(String[] args) throws InstantiationException, IllegalAccessException, IOException {
    Topology topology = new Topology(ExecuteType.LOCAL, WordCount.class.getName(),
        "WordCount");
    topology.setInput(WordInput.class, 1);
    topology.setOutput(WordOutput.class, 1);
    TopologySubmitter.submit(topology, args);
  }
}
