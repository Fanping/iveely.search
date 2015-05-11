package com.iveely.framework.language;

import java.io.*;
import java.nio.charset.StandardCharsets;
import java.nio.file.Files;
import java.nio.file.Path;
import java.nio.file.Paths;
import java.util.*;

/**
 * Created by Ahmed on 1/16/15.
 */
public class DetectTrainer {

    //the total number of trigrams
    int num = 0;
    //hashmap to store the trigrams
    HashMap<String, Double> trigrams = new HashMap<String, Double>();

    void createTrigrams(String text) {
        //clean the text from punctuations, new lines, and multiple spaces.
        text = text.replaceAll("[^\\p{L}]", " ");
        text = text.replaceAll("\n", " ");
        text = text.replaceAll("\\s+", " ");

        //create trigrams and count them.
        for (int i = 0; i < text.length() - 2; i++) {
            String trigram = text.substring(i, i + 3);
            num += 1;
            if (trigrams.containsKey(trigram)) {
                trigrams.put(trigram, trigrams.get(trigram) + 1);
            } else {
                trigrams.put(trigram, (double) 1);
            }
        }
    }

    void computeProbability() {
        //compute the probabilities of the trigrams.
        for (String trigram : trigrams.keySet()) {
            trigrams.put(trigram, (double) trigrams.get(trigram) / (double) num);
        }
    }

    void eliminateFrequencies(int num) {
        //eliminates all trigrams with a frequency less than or equal to the passing argument
        Iterator<String> ite = trigrams.keySet().iterator();
        while (ite.hasNext()) {
            String trigram = ite.next();
            if (trigrams.get(trigram) <= num) {
                double value = trigrams.get(trigram);
                ite.remove();
                num -= value;
            }
        }
    }

    public static void train(String[] args) {
        List<String> textList = new ArrayList<>();
        String text = new String();
        DetectTrainer train = new DetectTrainer();

        //the first argument represent the language we are training so here i will separate it.
        String language = args[0];
        //take the rest of the arguments since they are considered the training data files.
        String[] files = Arrays.copyOfRange(args, 1, args.length);
        //go through the files and process them
        for (String file : files) {
            Path path = Paths.get(file);
            try {
                textList = Files.readAllLines(path, StandardCharsets.UTF_8);
            } catch (IOException e) {
                e.printStackTrace();
            }
            for (String x : textList) {
                if (textList.size() == 1) {
                    text = x;
                } else {
                    text = text.concat(x + " ");
                }
            }
            //create the trigrams for the current file
            train.createTrigrams(text);

            text = "";
        }
        //eliminate less informative low frequency trigrams
        train.eliminateFrequencies(2);
        //calculate the probabilities for each trigram.
        train.computeProbability();

        BufferedWriter out = null;
        try {
            //write the language trigram trained data file
            out = new BufferedWriter(new OutputStreamWriter(new FileOutputStream(language + ".dat"), "UTF-8"));
            for (String key : train.trigrams.keySet()) {
                out.write(key + " " + train.trigrams.get(key));
                out.write("\n");
            }
            out.close();
        } catch (UnsupportedEncodingException e) {
            e.printStackTrace();
        } catch (FileNotFoundException e) {
            e.printStackTrace();
        } catch (IOException e) {
            e.printStackTrace();
        }

    }
}
